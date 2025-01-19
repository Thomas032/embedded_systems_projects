#include "config.h"

void writeToRegister(uint8_t data, int register_addr);
void resetDisplay(void);
void resetMatrix(int addr);
void displayTime(void);
bool buttonPressed(uint8_t address, int index);
void editTime(int delta, int matrixIndex);
void switchClockFormat();

/**
 * @brief Structure to represent the clock state.
 */
typedef struct main_clock

{
	volatile int hours_tens;   /**< The tens digit of the hours. */
	volatile int hours_ones;   /**< The ones digit of the hours. */
	volatile int minutes_tens; /**< The tens digit of the minutes. */
	volatile int minutes_ones; /**< The ones digit of the minutes. */
	volatile int seconds_tens; /**< The tens digit of the seconds. */
	volatile int seconds_ones; /**< The ones digit of the seconds. */
	volatile int format;	   /**< The format of the clock (12 or 24). */
} Clock;

typedef struct main_button
{
	volatile uint8_t address;
	volatile int index;
	volatile int counter;
} Button;

Clock clock = {0, 0, 0, 0, 0, 0, 24}; /**< The instance of the Clock structure. */

// button instances
Button mode_button = {MODE_BTN, 0, 0};
Button plus_button = {PLUS_BTN, 1, 0};
Button minus_button = {MINUS_BTN, 2, 0};
Button edit_button = {EDIT_BTN, 3, 0};

bool editting = false;
int edit_matrix_index = 0;
bool am = true;

/**
 * @brief Sets up the communication and initial pin states.
 */
void setup(void)
{
	// setup of the ports
	// set ports as output

	DDRA = 0xFF;
	PORTA = 0xFF;

	MATRIX_DATA_CONF = 0xFF;
	MATRIX_CONTROL_CONF = 0xFF;

	// set button port as input
	// buttons
	BUTTON_PORT_CONF = 0x00;
	BUTTON_DATA_OUT = 0xFF;

	// set the clock timer
	uint16_t compare_value = (F_CPU / (1024 * TICK_INTERVAL)) - 1; // dynamic max. value calculation
	TCCR1B = (1 << WGM12) | (1 << CS12) | (1 << CS10);			   // CTC mode, prescaler 1024
	TCNT1 = 0;													   // reset timer
	OCR1A = compare_value;										   // set the compare value
	TIMSK |= (1 << OCIE1A);										   // enable interrupt on compare match
	sei();														   // enable global interrupts
}

/**
 * @brief The main method of the program.
 */
void main(void)
{
	setup();

	resetDisplay();

	while (1)
	{
		// check for button press so that time and format can be adjusted -> TODO
		displayTime();

		if (buttonPressed(mode_button.address, mode_button.index))
		{
			mode_button.counter++;

			if (mode_button.counter % BTN_STANDARD_TRIGGER == 0)
			{
				if (editting)
				{
					edit_matrix_index++;
					if (edit_matrix_index > 5)
					{
						edit_matrix_index = 0;
					}

					PORTA = 0x00;
					_delay_ms(250);
					PORTA = 0xFF;
				}
				else
				{
					switchClockFormat();
				}
				mode_button.counter = 0;
			}
		}

		if (buttonPressed(plus_button.address, plus_button.index))
		{
			plus_button.counter++;
			if (plus_button.counter % BTN_STANDARD_TRIGGER == 0)
			{
				if (editting)
				{
					editTime(1, edit_matrix_index);
					resetMatrix(edit_matrix_index);
				}

				plus_button.counter = 0;
			}
		}

		if (buttonPressed(minus_button.address, minus_button.index))
		{
			minus_button.counter++;
			if (minus_button.counter % BTN_STANDARD_TRIGGER == 0)
			{
				if (editting)
				{
					editTime(-1, edit_matrix_index);
					resetMatrix(edit_matrix_index);
				}

				minus_button.counter = 0;
			}
		}

		if (buttonPressed(edit_button.address, mode_button.index))
		{
			edit_button.counter++;
			if (edit_button.counter % BTN_STANDARD_TRIGGER == 0)
			{
				editting = !editting;
				edit_button.counter = 0;
			}
		}
	}
}

void editTime(int delta, int matrixIndex)
{

	// increment the cl
	switch (matrixIndex)
	{
	case 0:
		clock.hours_tens += delta;
		if (clock.hours_tens < 0)
			clock.hours_tens = 0;
		break;
	case 1:
		clock.hours_ones += delta;
		if (clock.hours_ones < 0)
			clock.hours_ones = 0;
		break;
	case 2:
		clock.minutes_tens += delta;
		if (clock.minutes_tens < 0)
			clock.minutes_tens = 0;
		break;
	case 3:
		clock.minutes_ones += delta;
		if (clock.minutes_ones < 0)
			clock.minutes_ones = 0;
		break;
	case 4:
		clock.seconds_tens += delta;
		if (clock.seconds_tens < 0)
			clock.seconds_tens = 0;
		break;
	case 5:
		clock.seconds_ones += delta;
		if (clock.seconds_ones < 0)
			clock.seconds_ones = 0;
		break;
	}

	if (clock.seconds_ones == 10)
	{
		clock.seconds_ones = 0;
		clock.seconds_tens++;
	}
	if (clock.seconds_tens == 6)
	{
		clock.seconds_tens = 0;
		clock.minutes_ones++;
	}
	if (clock.minutes_ones == 10)
	{
		clock.minutes_ones = 0;
		clock.minutes_tens++;
	}
	if (clock.minutes_tens == 6)
	{
		clock.minutes_tens = 0;
		clock.hours_ones++;
	}
	if (clock.hours_ones == 10)
	{
		clock.hours_ones = 0;
		clock.hours_tens++;
	}

	// Check clock format and adjust hours accordingly
	if (clock.format == 12)
	{
		if (clock.hours_tens == 1 && clock.hours_ones == 3)
		{
			// If it's currently 13 (1 PM), reset to 1 (1 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 1;
			am = !am;
		}
		else if (clock.hours_tens > 1)
		{
			// If it's currently greater than 12, reset to 1 (1 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 1;
		}
	}
	else if (clock.format == 24)
	{
		if (clock.hours_tens == 2 && clock.hours_ones == 4)
		{
			// If it's currently 24 (midnight), reset to 0 (12 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 0;
		}
		else if (clock.hours_tens > 2)
		{
			// If it's currently greater than 23, reset to 0 (12 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 0;
		}
	}
}

bool buttonPressed(uint8_t address, int index)
{
	if ((BUTTON_DATA_IN & address) >> index != 0)
	{
		// button pressed
		return false;
	}
	return true;
}

void switchClockFormat()
{
	if (clock.format == 24)
	{
		clock.format = 12;
	}
	else
	{
		clock.format = 24;
	}

	// if time format is ewual to 12h -> update the current state of the clock
	// the state of the 12h clock should also reflect if it is AM or PM -> simply add or delete LED to some segment
	if (clock.format == 12)
	{
		int hours = (clock.hours_tens * 10 + clock.hours_ones);

		if (hours < 13)
		{
			am = true;
		}

		// possible time = 23:00 -> shift to 11PM
		if (clock.hours_tens == 2)
		{
			int converted = hours - 12;

			int tens = converted / 10;
			int ones = converted % 10;

			clock.hours_tens = tens;
			clock.hours_ones = ones;

			am = false;
		}
	}

	if (clock.format == 24)
	{
		// check if time is pm or am
		if (!am)
		{
			// time is AM -> can keep the values
			// time is PM -> need to convert to numbers > 12
			int converted = (clock.hours_tens * 10 + clock.hours_ones) + 12;

			int tens = converted / 10;
			int ones = converted % 10;

			clock.hours_tens = tens;
			clock.hours_ones = ones;
		}
	}
}

/**
 * @brief Displays the current time on the matrix.
 */
void displayTime()
{
	// function to display current state of the clock
	for (int i = 0; i < 8; i++)
	{
		// for each of the columns
		// get "row" data from the current time variables -> HH:MM:SS

		uint8_t hours_tens_data = MAP[clock.hours_tens][7 - i];
		uint8_t hours_ones_data = MAP[clock.hours_ones][7 - i];
		uint8_t minutes_ones_data = MAP[clock.minutes_ones][7 - i];
		uint8_t minutes_tens_data = MAP[clock.minutes_tens][7 - i];
		uint8_t seconds_tens_data = MAP[clock.seconds_tens][7 - i];
		uint8_t seconds_ones_data = MAP[clock.seconds_ones][7 - i];

		// set the colons to the font
		uint8_t colon = 0b00100100;
		uint8_t format_sign = 0b11000000;
		uint8_t am_sign = 0b00001100;
		uint8_t pm_sign = 0b00000011;

		// check if the current displayed row is 0, if yes add colons to the font
		if (i == 0)
		{
			minutes_ones_data |= colon;
			hours_ones_data |= colon;
		}

		if (i == 0 && clock.format == 12)
		{
			seconds_ones_data |= format_sign;

			if (am)
			{
				seconds_ones_data |= am_sign;
			}
			else
			{
				seconds_ones_data |= pm_sign;
			}
		}

		// write data to appropriate registers -> data bust be negated -> display active in log. 0
		writeToRegister(hours_tens_data ^ 0xFF, 0);
		writeToRegister(hours_ones_data ^ 0xFF, 1);
		writeToRegister(minutes_tens_data ^ 0xFF, 2);
		writeToRegister(minutes_ones_data ^ 0xFF, 3);
		writeToRegister(seconds_tens_data ^ 0xFF, 4);
		writeToRegister(seconds_ones_data ^ 0xFF, 5);

		// activate the column register
		writeToRegister((0x01 << i) ^ 0xFF, MATRIX_COLUMN_INDEX);

		// wait for the display period
		_delay_us(250);

		// reset the column register
		writeToRegister(0xFF, MATRIX_COLUMN_INDEX);
		_delay_us(1);
	}
}

/**
 * @brief Resets the entire display.
 */
void resetDisplay()
{
	// function to reset the whole display
	for (int i = 7; i >= 0; i--)
	{
		resetMatrix(i);
	}
}

/**
 * @brief Resets a matrix.
 *
 * @param addres The address of the matrix.
 */
void resetMatrix(int addres)
{
	// function to reset one matrix
	for (int j = 0; j < 6; j++)
	{
		writeToRegister(0xFF, j);
	}

	writeToRegister((0x01 << addres) ^ 0xFF, MATRIX_COLUMN_INDEX);
	_delay_us(1);
	writeToRegister(0xFF, MATRIX_COLUMN_INDEX);
	_delay_us(1);
}

/**
 * @brief ISR for Timer 1 for updating the clock.
 */
ISR(TIMER1_COMPA_vect)
{
	if (editting)
	{
		// if user is edditing the current state of the clock ->  do not increment timer
		return;
	}

	// increment the clock
	clock.seconds_ones++;

	if (clock.seconds_ones == 10)
	{
		clock.seconds_ones = 0;
		clock.seconds_tens++;
	}
	if (clock.seconds_tens == 6)
	{
		clock.seconds_tens = 0;
		clock.minutes_ones++;
	}
	if (clock.minutes_ones == 10)
	{
		clock.minutes_ones = 0;
		clock.minutes_tens++;
	}
	if (clock.minutes_tens == 6)
	{
		clock.minutes_tens = 0;
		clock.hours_ones++;
	}
	if (clock.hours_ones == 10)
	{
		clock.hours_ones = 0;
		clock.hours_tens++;
	}

	// Check clock format and adjust hours accordingly
	if (clock.format == 12)
	{
		if (clock.hours_tens == 1 && clock.hours_ones == 3)
		{
			// If it's currently 13 (1 PM), reset to 1 (1 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 1;
			am = !am;
		}
		else if (clock.hours_tens > 1)
		{
			// If it's currently greater than 12, reset to 1 (1 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 1;
		}
	}
	else if (clock.format == 24)
	{
		if (clock.hours_tens == 2 && clock.hours_ones == 4)
		{
			// If it's currently 24 (midnight), reset to 0 (12 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 0;
		}
		else if (clock.hours_tens > 2)
		{
			// If it's currently greater than 23, reset to 0 (12 AM)
			clock.hours_tens = 0;
			clock.hours_ones = 0;
		}
	}
}

/**
 * @brief Writes data to a register.
 *
 * @param data The data to be written.
 * @param register_addr The address of the register.
 */
void writeToRegister(uint8_t data, int register_addr)
{
	// function that writes data to register
	// convert data to inverted form
	// set data to data port
	MATRIX_DATA_OUT = data;

	// activate the appropriate register address
	MATRIX_CONTROL_OUT = (0x01 << register_addr);
	_delay_us(1);

	// deactivate the register
	MATRIX_CONTROL_OUT = 0x00;
	_delay_us(1);
}