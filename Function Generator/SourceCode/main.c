/**
 * @file main.c
 * @brief Main program file for the signal generation and communication.
 */

#include "config.h"

// custom struct for better representation of user data
typedef struct {
	char *channel;
	char *parameter;
	char *value;
} UserInput;

// custom struct for better management of the channel related values
typedef struct {
	int function;
	int frequency;
	int amplitude;
	uint32_t modulo;
	volatile int internalCounter;
	volatile int outputCounter;
} Channel;

// function prototypes
void setChannelVariables(Channel *channel, UserInput parsedInput);
void parseUserInput(UserInput *result, char* data);
bool GetCommandValidity(UserInput *input);
void CH1OutputFunction(void);
void CH2OutputFunction(void);
uint32_t GetModulo(uint32_t frequency);
void outputToDAC(volatile uint8_t *port, uint8_t value);
void fillSineTable();
void fillLinearTable();

// Output function predefined arrays

float sineTable[SAMPLING_RATE];
float lineTable[SAMPLING_RATE];
//float roofTable[SAMPLING_RATE];

// blank representation of the channels
//			  fnc	freq	  amp	 mod   iCntr  oCntr
Channel CH1 = {0,   1,		  210, 	  0,  	0,  	 0};
Channel CH2 = {0,	0,	  	  210,	  0,	0,		 0};


// blank data for the UserInput
UserInput userData = {"", "", ""};

// user buffer related variables
char buffer[MAX_CMD_LEN];
int bufferIndex = 0;
char lastChar;


/**
 * @brief Set up the basic port and communication.
 * This function initializes the basic port and communication settings.
 */
void setup(void)
{
    // Setup the communication protocol.
    usart_setup();
	
    CH1_CONF = 0xFF; // Setup of channel 1.
    CH2_CONF = 0xFF; // Setup of channel 2.

	// Configure Timer 1 in CTC mode with the prescaler = 1
    TCCR1B = (1 << WGM12) | (1 << CS10); // CTC mode and prescaler equal to 1
	TCNT1 = 0; // reset of the timer
    OCR1A = 255;  // Set TOP value for CTC mode
    TIMSK |= (1 << OCIE1A);  // Enable Timer 1 interrupt on Compare Match A

  
	// fill in the function tables
	fillSineTable();
	fillLinearTable();
	
	// calulate the initial modulos
	CH1.modulo = GetModulo(CH1.frequency);
	CH2.modulo = GetModulo(CH2.frequency);

    // Enable global interrupts
	sei();
	
}
/**
 * @brief The main entry point of the program.
 * @return The program exit status.
 */
int main(void)
{
    // Initialization part of the program.
	setup();
	
	printf("Welcome to a DigiWave generator!\n");
	printf("Each command for the DAC has to have this structure: \n");
	printf("\tCHANNEL:PARAMETER:VALUE (eg. CH1:TYPE:SIN)\n");

    // The main loop.
    while (1)
    {
		printf("Enter DAC command: ");
		
		while((int)lastChar != 13)
		{
			lastChar = usart_getchar();
			
			if(lastChar != 13)
			{
				buffer[bufferIndex] = lastChar;
		
				if(bufferIndex < MAX_CMD_LEN - 1)
				{
					bufferIndex++;
				}
				else
				{
					printf("Maximum command length exceeded, reseting to empty buffer ...\n");
					memset(buffer, 0, MAX_CMD_LEN);
					bufferIndex = 0;
				}
			}
		}
				
		lastChar = NULL;
		
		// enter pressed
		parseUserInput(&userData, buffer);

		if(!GetCommandValidity(&userData))
		{
			// invalid command -> clear the buffer
			printf("Invalid command entered. Try again.\n");
			memset(buffer, 0, MAX_CMD_LEN);
			bufferIndex = 0;
			continue;
		}
		
		if (strcmp(userData.channel, "CH1") == 0)
		{
			setChannelVariables(&CH1, userData);
		}
		
		if (strcmp(userData.channel, "CH2") == 0)
		{
			setChannelVariables(&CH2, userData);
		}
		
		// assign the modulos
		CH1.modulo = GetModulo(CH1.frequency);
		CH2.modulo = GetModulo(CH2.frequency);
		
		memset(buffer, 0, MAX_CMD_LEN); // empty the buffer array
		bufferIndex = 0; // reset the buffer index
    }
	
    return 0;
}

/**
 * @brief Set channel-specific variables based on user input.
 * @param channel Pointer to the Channel struct.
 * @param parsedInput UserInput struct containing parsed user input.
 */
void setChannelVariables(Channel *channel, UserInput parsedInput)
{
    if (strcmp(parsedInput.parameter, "TYPE") == 0)
    {
        // set the current function
        if (strcmp(parsedInput.value, "SIN") == 0)
        {
            channel->function = 0;
        }
        else if (strcmp(parsedInput.value, "LINE") == 0)
        {
            channel->function = 1;
        }
        else if (strcmp(parsedInput.value, "SQR") == 0)
        {
            channel->function = 2;
        }
    }
    else
    {
        // FREQ or AMP -> need to convert the char* to int
        int value = atoi(parsedInput.value);

        if (strcmp(parsedInput.parameter, "FREQ") == 0)
        {
            channel->frequency = value; // frequency is by default in Hz
        }
        else if (strcmp(parsedInput.parameter, "AMP") == 0)
        {
            channel->amplitude = convertAmplitude(value); // convert from mV to interval <0;210>
        }
    }
}


/**
 * @brief Parse user input and return a UserInput struct.
 * @param input User input string.
 * @return Parsed UserInput struct.
 */
void parseUserInput(UserInput *result, char* data)
{
	// init the fields as empty
	result->channel = result->parameter = result->value = NULL;

	int id = 0;

	// split the user input with the ':' delimeter
	char *token = strtok(data, ":");
	
	// go through the split string
    while (token != NULL) {
		if(id == 0)
		{
			result->channel = token;
		}
		if(id == 1)
		{
			result->parameter = token;
		}
		if(id == 2)
		{
			result->value = token;
		}
        token = strtok(NULL, ":");
		id++;
    }
}


/**
 * @brief Check the validity of a user command.
 * @param input UserInput struct to be validated.
 * @return True if the command is valid, false otherwise.
 */
bool GetCommandValidity(UserInput *input)
{
	if (input->channel == NULL || input->parameter == NULL || input->value == NULL)
	{
		// invalid input -> incomplete command
		return false;
	}
	
	bool valid = false;
	
	if (strcmp(input->channel, "CH1") == 0 || strcmp(input->channel, "CH2") == 0)
	{
		// channel is entered correctly
		valid = true;
	}
	
	if(strcmp(input->parameter, "TYPE") == 0 || strcmp(input->parameter, "FREQ") == 0 || strcmp(input->parameter, "AMP") == 0)
	{
		// parameter has the correct type
		valid = true;
	}
	else
	{
		valid = false;
	}
	
	
	// check if the entered type is correct
	if(strcmp(input->parameter, "TYPE") == 0)
	{
		if(strcmp(input->value, "SIN") == 0 || strcmp(input->value, "LINE") == 0 || strcmp(input->value, "SQR") == 0)
		{
			valid = true;
		}
		else
		{
			valid = false;
		}
	}
	else
	{
		// value has to be a number as the param is either freq or sin
		// convert the char* to int -> ASCII to integer
		int valueNumber = atoi(input->value);
		
		if(strcmp(input->parameter, "AMP") == 0)
		{
			if(valueNumber > 0 && valueNumber <= ANALOG_MAX_AMP)
			{
				valid = true;
			}
			else
			{
				valid = false;
			}
		}
		else
		{
			// input.parameter == FREQ
			if(valueNumber > 0)
			{
				valid = true;
			}
			else
			{
				valid = false;
			}
		}
	}
	
	return valid;
}

/**
 * @brief ISR for Timer 1 for generating the signals on both channels
 */
ISR(TIMER1_COMPA_vect)
{
	CH1.internalCounter++;
	CH2.internalCounter++;
	
	// CH1 code
	if(CH1.internalCounter >= CH1.modulo)
	{
		// if it is time to generate -> reset internal counter
		CH1.internalCounter = 0;

		// output the proper values for channel one
		CH1OutputFunction();
		
		CH1.outputCounter++; // increase the array index

		if(CH1.outputCounter > SAMPLING_RATE - 1)
		{
			// if index out of bounds -> reset it
			CH1.outputCounter = 0;
		}
	}
	
	// CH2 code
	if(CH2.internalCounter >= CH2.modulo)
	{
		// if it is time to generate -> reset the internal counter
		CH2.internalCounter = 0;
		
		// output the proper values for ch2
		CH2OutputFunction();
		
		CH2.outputCounter++; // increase the array index
		
		if(CH2.outputCounter > SAMPLING_RATE - 1)
		{
			// if the array index out of bounds -> reset it
			CH2.outputCounter = 0;
		}
	}
}

/**
 * @brief Output function for Channel 1.
 */
void CH1OutputFunction()
{

	int yCH1 = 0;

	if(CH1.function == 0)
	{
		// sine function
		int virtualZero = CH1.amplitude / 2;
		yCH1 = virtualZero + (CH1.amplitude / 2) * sineTable[CH1.outputCounter];
		outputToDAC(&CH1_OUT, (yCH1));
	}
	
	if(CH1.function == 1)
	{
		// line function
		float out = lineTable[CH1.outputCounter]; // temporary variable due to incompatible variabe type of yCH1
		outputToDAC(&CH1_OUT, CH1.amplitude * out);
	}
	if(CH1.function == 2)
	{
		// square function -> duty cycle 50%
		if(CH1.outputCounter < (SAMPLING_RATE / 2))
		{
			outputToDAC(&CH1_OUT, CH1.amplitude);
		}
		else
		{
			outputToDAC(&CH1_OUT, 0);
		}
	}
}


/**
 * @brief Output function for Channel 2.
 */
 
void CH2OutputFunction()
{
	int yCH2 = 0;
	if(CH2.function == 0)
	{
		// sine function
		int virtualZero = CH2.amplitude / 2;
		yCH2 = virtualZero + (CH2.amplitude / 2) * sineTable[CH2.outputCounter];
		outputToDAC(&CH2_OUT, (yCH2));
	}
	
	if(CH2.function == 1)
	{
		// linear function
		float out = lineTable[CH2.outputCounter];
		outputToDAC(&CH2_OUT, CH2.amplitude * out);
	}
	
	if(CH2.function == 2)
	{
		// square function -> duty cycle 50%
		if(CH2.outputCounter < (SAMPLING_RATE / 2))
		{
			outputToDAC(&CH2_OUT, CH2.amplitude);
		}
		else
		{
			outputToDAC(&CH2_OUT, 0);
		}
	}
}

/**
 * @brief Function that fills the global sine table with data to speed up the runtime.
 * the calculated value is in range <-1; 1> => need to set up the virtual zero
 */
void fillSineTable()
{
	for(int i=0; i < SAMPLING_RATE; i++)
	{
		// convert deg to rad and calculate the sine of that value
		sineTable[i] = sin((M_PI / 180.0) * i);
	}
}

/**
 * @brief Function that fills the global linear table with data to speed up the runtime.
 */
void fillLinearTable()
{
	for(int i=0; i < SAMPLING_RATE; i++)
	{
		lineTable[i] = (i * (1.0/SAMPLING_RATE)); // 1/SAMPLING_RATE = 0.00277
	}
}


/**
 * @brief Function that gets the number of ticks between each DAC output to controll the frequency.
 * @param frequency = desired frequency of the signal
 * @param sampleRate = the resolution of the output signal
 */
uint32_t GetModulo(uint32_t frequency) {
    // Calculate the number of ticks for a given frequency 
    uint32_t ticks = ((F_CPU / 256) / frequency) / SAMPLING_RATE; // 256 = number of possible outputs of the DAC -> 2**8
    return ticks;
}

/**
 * @brief function that outputs a given value to a predefined port.
 * @param port = address to an output port
 * @param value = decimal 8 bit value to output on the given port
 */
void outputToDAC(volatile uint8_t *port, uint8_t value)
{
	// assign the value to the port
	*port = value;
}


/**
 * @brief Converts the amplitude from mV to decimal [0, DIGITAL_MAX_AMP].
 * @param oldAmplitude The amplitude in mV.
 * @return The converted amplitude in the range [0, DIGITAL_MAX_AMP].
 */
int convertAmplitude(int oldAmplitude)
{
    return (oldAmplitude * DIGITAL_MAX_AMP) / (ANALOG_MAX_AMP);
}