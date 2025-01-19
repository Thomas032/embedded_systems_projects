#include "config.h"

// custom data structure for effectively stroing user input
typedef struct
{
	char operation;
	uint16_t operand_0;
	uint16_t operand_1;
} UserInput;

// function prototypes for I2C
void startBit(void);
void stopBit(void);
void sendBit(int bit);
void sendByte(uint8_t byte);
int readBit(void);
void tick(bool wait);
void sendAddr(int readWrite);
uint8_t readByte(void);
uint8_t readFromEEPROM(uint16_t address, int *errorFlag);
int writeToEEPROM(uint16_t address, uint8_t data);
int sendByteAndCheckAck(uint8_t byte);

// function protoypes for serial comm.
int parseUserInput(UserInput *result, char *data);
int getCommandValidity(UserInput *input);
int getAddr(char *data, uint16_t *saveAddr);

// global objects
UserInput userData = {'\0', 0x0000, 0x0000};

// user buffer related variables
char buffer[MAX_CMD_LEN];
int bufferIndex = 0;
char lastChar = '\0';

/**
 * @brief Method for setting up the communication as well as the initial ppin states.
 */
void setup(void)
{
	// serial communication setup
	usart_setup();

	// setup SDA and SCL as output pins
	I2C_CONF = (SDA | SCL);
}

/**
 * @brief Main method of the program
 */
void main(void)
{
	setup();
	printf('\033[2J');
	printf("Welcome to E2PROM interface!\n");
	printf("Each command has to have this structure: \n");
	printf("\t W ADDR DATA or R ADDR  (eg. W 0x0000 0xFF)\n");

	while (1)
	{
		printf("Enter an EEPROM command: ");

		while ((int)lastChar != ENTER_CODE)
		{
			// while not enter

			// save the currently pressed character
			lastChar = usart_getchar();

			buffer[bufferIndex] = lastChar;

			if (bufferIndex < MAX_CMD_LEN - 1)
			{
				// if the buffer index in bounds -> add
				bufferIndex++;
			}
			else
			{
				// buffer would be out of bounds -> reset buffer
				printf("Maximum command length exceeded, resetting to empty buffer ...\n");
				memset(buffer, 0, MAX_CMD_LEN);
				bufferIndex = 0;
			}
		}

		// command was submitted -> parse the buffer

		lastChar = '\0'; // anulate the last charr
		buffer[bufferIndex] = lastChar;

		int valid = parseUserInput(&userData, buffer);

		// reset the buffer array and index
		memset(buffer, 0, MAX_CMD_LEN);
		bufferIndex = 0;

		if (valid == 0)
		{
			// inpit was invalid -> show error and return to start
			printf("Invalid command. Try again...\n");
			continue;
		}
		// printf("operand 0 = %d\n", userData.operand_0);
		//  validate the input
		if (getCommandValidity(&userData) == 0)
		{
			printf("Invalid command. Try again...\n");
			continue;
		}

		// input fully valid -> check for opperation and issue the I2C commands
		if (userData.operation == 'R')
		{
			// read operation
			printf("Reading from address %x\n", userData.operand_0);
			int errorFlag = 0;
			uint8_t data = readFromEEPROM(userData.operand_0, &errorFlag);
			if (errorFlag == 1)
			{
				printf("Error occured while reading from EEPROM\n");
			}
			else
			{
				printf("\nData read: %x\n", data);
			}
		}
		else
		{
			// write operation
			printf("Writing %x to address %x\n", userData.operand_1, userData.operand_0);
			int success = writeToEEPROM(userData.operand_0, userData.operand_1);

			if (success == 0)
			{
				printf("Error occured while writing to EEPROM\n");
			}
			else
			{
				printf("Data written successfully\n");
			}
		}
	}
}

/**
 * @brief Method for writing a byte to the EEPROM
 * @param address The address to write to
 * @param data The data to write
 */
int writeToEEPROM(uint16_t address, uint8_t data)
{
	// function for writing a byte to the EEPROM
	// writing works by sending 2x 8 it address and than the data
	int ack = communicationStart(WRITE_VAL);

	if (DEBUG_OUTPUT)
	{
		printf("Write ack: %d \n", ack);
	}

	if (ack != 0)
	{
		// raise an error when ACK not received
		return 0;
	}

	// split the address into 2 8 bit numbers
	uint8_t msbAddr = (uint8_t)(address >> 8);
	uint8_t lsbAddr = (uint8_t)(address & 0xFF);

	// send the memory cell address by sending 2 bytes
	if ((sendByteAndCheckAck(msbAddr) != 0) || (sendByteAndCheckAck(lsbAddr) != 0))
	{
		return 0;
	}

	// send the actual data and send stop bit
	if (sendByteAndCheckAck(data) != 0)
	{
		return 0;
	}

	stopBit();

	// everything went well -> return 1 to indicate success
	return 1;
}

/**
 * @brief Method for sending the address user want to read to the EEPROM
 */
int dummyWrite(uint16_t address)
{
	// function for writing a byte to the EEPROM
	// writing works by sending 2x 8 it address and than the data
	int ack = communicationStart(WRITE_VAL);

	if (ack != 0)
	{
		// raise an error when ACK not received
		return 0;
	}

	// split the address into 2 8 bit numbers
	uint8_t msbAddr = (uint8_t)(address >> 8);
	uint8_t lsbAddr = (uint8_t)(address & 0xFF);

	// send the memory cell address by sending 2 bytes
	if ((sendByteAndCheckAck(msbAddr) != 0) || (sendByteAndCheckAck(lsbAddr) != 0))
	{
		return 0;
	}
	// everything went well -> return 1 to indicate success
	return 1;
}

/**
 * @brief Method for reading a byte from the EEPROM
 * @param address The address to read from
 * @param errorFlag Pointer to the error flag variable
 */
uint8_t readFromEEPROM(uint16_t address, int *errorFlag)
{
	dummyWrite(address);

	startBit();
	sendAddr(READ_VAL);
	int ack = readBit();
	if (ack != 0)
	{
		// set an error flag to true
		*errorFlag = 1;
	}

	uint8_t data = readByte();

	stopBit();

	return data;
}

/**
 * @brief Method for sending a start bit to the slave
 */
void startBit(void)
{
	// function for sending the start bit condition to the I2C slave

	// SDA and SCL on
	I2C_OUT = SDA | SCL;
	// wait a bit
	_delay_us(I2C_DELAY);
	// SDA off
	I2C_OUT &= ~(SDA);
	// delay
	_delay_us(I2C_DELAY);
	// SCL off
	I2C_OUT &= ~SCL;
	// wait
	_delay_us(I2C_DELAY);
	printf("|START");
}

/**
 * @brief Method for sending a stop bit to the slave
 */
void stopBit(void)
{
	// function for sending the stop bit condition to the I2C slave

	// SDA off and SCL off
	I2C_OUT &= ~(SCL | SDA);
	// wait a bit
	_delay_us(I2C_DELAY);
	// SCL ON
	I2C_OUT |= SCL;
	// delay
	_delay_us(I2C_DELAY);
	// SDA ON
	I2C_OUT |= SDA;
	// delay
	_delay_ms(I2C_DELAY);
	printf("STOP|");
}

/**
 * @brief Method for starting the communication with the EEPROM
 * @param readWrite The read/write bit to be sent
 * @return Int statig whether or not the mcu received an ACK bit
 */
int communicationStart(int readWrite)
{
	startBit();
	sendAddr(readWrite);
	int ack = readBit();
	return ack;
}

/**
 * @brief Method for sending an address and the rad write bit
 * @param readWrite The read/write bit to be sent
 */
void sendAddr(int readWrite)
{
	// function for sending an address and the rad write bit
	uint8_t completeByte = SLAVE_ADDR | readWrite;
	sendByte(completeByte);
}

/**
 * @brief Method for sending one bit of DATA on the I2C line
 * @param bit The bit to be sent on the bus
 */
void sendBit(int bit)
{
	printf("%d", bit);
	if (bit == 0)
	{
		I2C_OUT &= ~SDA;
	}
	else
	{
		I2C_OUT |= SDA;
	}

	// wait a little and tick
	tick(true);
}

/**
 * @brief Method for sending one byte to the I2C bus
 * @param byte The actual data to send
 * @param MSBFirst Determines whether the most significant bit should go  firs
 */
void sendByte(uint8_t byte)
{
	printf("|");
	for (int i = 7; i >= 0; i--)
	{
		if (isHigh(byte, i) == 1)
		{
			// send log. 1
			sendBit(1);
		}
		else
		{
			// send log. 0
			sendBit(0);
		}
	}
	printf("|");
}

/**
 * @brief Method for sending one byte to the I2C bus and checking for ACK
 * @param byte The actual data to send
 * @param MSBFirst Determines whether the most significant bit should go  firs
 * @return Int statig whether or not the mcu received an ACK bit
 */
int sendByteAndCheckAck(uint8_t byte)
{
	sendByte(byte);
	int ack = readBit();
	if (DEBUG_OUTPUT)
	{
		printf("Send byte and check ack: %d \n", ack);
	}
	return ack;
}

/**
 * @brief Method for reading a bit from the I2C bus
 */
int readBit(void)
{
	// SDA high
	I2C_OUT |= SDA;

	// I2c_conf SDA pin TO INPUT ->* SDA bit set to zero
	I2C_CONF &= ~(SDA);

	// clock tick -> keep the clock high and read the data
	I2C_OUT |= (SCL);
	_delay_us(1);

	// read data while the clock is high
	int bit = (I2C_IN & SDA);

	// set the clock down
	I2C_OUT &= ~(SCL);

	// set the original SDA state
	I2C_OUT |= SDA;

	I2C_CONF |= (SDA);

	printf("%d", bit);

	if (bit == 0)
	{
		return 0;
	}

	return 1;
}

uint8_t readByte()
{
	// function for reading a byte from the I2C bus
	uint8_t byte = 0x00;
	printf("|[");
	for (int i = 7; i >= 0; i--)
	{
		// read the bit
		int bit = readBit();
		if (bit != 0)
		{
			byte = setBit(byte, i);
		}
		else
		{
			byte = nullBit(byte, i);
		}
	}
	printf("]");
	printf("|");
	return byte;
}

/**
 * @brief Method for ticking with the clock
 * @param wait Parameter determining if the method waits for the previsously set signal to stabilise
 */
void tick(bool wait)
{
	if (wait)
	{
		// wait for the previously set signal to stabilise on the pin
		_delay_ms(I2C_DELAY / 2);
	}

	I2C_OUT |= SCL;
	_delay_us(I2C_DELAY);
	I2C_OUT &= ~(SCL);
	_delay_us(I2C_DELAY);
}

/**
 * @brief Parse user input and return a UserInput struct with the appropriate data.
 * @param result pointer to the global UserInput variable
 * @param data User input string.
 */
int parseUserInput(UserInput *result, char *data)
{
	// reset the current userInput values
	result->operation = '\0';				   // Initialize to a default char value
	result->operand_0 = result->operand_1 = 0; // Initialize to a default integer value

	int splitID = 0;

	char *token = strtok(data, " ");

	while (token != NULL)
	{
		// printf("Token %d: %s\n", splitID, token ? token : "NULL");
		if (splitID == 0)
		{
			// either R or W
			if ((token[0] == 'R' || token[0] == 'W') && token[1] == '\0')
			{
				// printf("Assigning operation: %s\n", token);
				result->operation = token[0];

				if (DEBUG_OUTPUT)
				{
					printf("Assigned operation character %c\n'", result->operation);
				}
			}
			else
			{
				// printf("Invalid operation\n");
				return 0;
			}
		}
		else
		{
			uint16_t addr;
			if (getAddr(token, &addr) != 0)
			{
				// conversion occured successfully -> save
				if ((splitID == 1))
				{
					result->operand_0 = addr;
				}
				else if ((splitID == 2))
				{
					result->operand_1 = addr;
				}
				else
				{
					// invalid address length
					return 0;
				}
			}
			else
			{
				// error occured -> return invalid flag
				if (DEBUG_OUTPUT)
				{
					printf("addr error\n");
				}
				return 0;
			}
		}
		token = strtok(NULL, " ");
		splitID++;
	}

	// everything went smoothly -> success flag
	return 1;
}

/**
 * @brief CHeck the validity of user data before sending
 * @param input pointer to the UserInput datatype storing all input related vars
 * @return int assessing the validity of the user input
 */
int getCommandValidity(UserInput *input)
{
	if (input->operand_0 < MIN_ADDR || input->operand_0 > MAX_ADDR)
	{
		if (DEBUG_OUTPUT)
		{
			printf("base case 1\n");
		}
		return 0;
	}

	// more compley checks
	if (input->operation == 'R')
	{
		// read operation issued
		return 1;
	}

	if (input->operation == 'W')
	{
		// write operation issued
		if (input->operand_1 != NULL)
		{
			if (MIN_ADDR <= input->operand_1 && input->operand_1 <= MAX_ADDR)
			{
				return 1;
			}
		}
	}
	return 0;
}

/**
 * @brief Convert the string representation of 16 bit addr. to uint_8.
 * @param data String representation of the address
 * @param saveAddr pointer to the final variable to save the data to
 * @return int stating the status of conversion
 */
int getAddr(char *data, uint16_t *saveAddr)
{
	if (data == NULL)
	{
		return 0;
	}

	// convert a char* to a 16 base number

	char *ptr;
	unsigned long ret;

	ret = strtoul(data, &ptr, 16);

	// convert the data back to unsigned 16 bit int
	uint16_t result = (uint16_t)ret;
	// save data to the final specified destination
	*saveAddr = result;
	return 1;
}