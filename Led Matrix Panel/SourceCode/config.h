#ifndef CONFIG_H
#define CONFIG_H

// Include necessary libraries
#include <stdio.h>
#include <math.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <avr/io.h>
#include <util/delay.h>
#include <avr/interrupt.h>

// include custom liraries
#include "bin_lib.h"

// global registers for DDR, PORT, PIN
#define MATRIX_DATA_CONF (DDRE)
#define MATRIX_CONTROL_CONF (DDRB)
#define MATRIX_DATA_OUT (PORTE)
#define MATRIX_CONTROL_OUT (PORTB)

#define BUTTON_PORT_CONF (DDRC)
#define BUTTON_DATA_OUT (PORTC)
#define BUTTON_DATA_IN (PINC)

// Global pin setup
#define MATRIX_CONTROL_0 0x01
#define MATRIX_CONTROL_1 0x02
#define MATRIX_CONTROL_2 0x04
#define MATRIX_CONTROL_3 0x08
#define MATRIX_CONTROL_4 0x10
#define MATRIX_CONTROL_5 0x20
#define MATRIX_CONTROL_8 0x40

#define MATRIX_COLUMN_INDEX 6

#define DISPLAY_DELAY 250
#define STANDARD_DELAY 1

#define BTN_STANDARD_TRIGGER 100
#define BTN_LONG_TRIGGER 250

// button setup
#define MODE_BTN 0x01
#define PLUS_BTN 0x02
#define MINUS_BTN 0x04
#define EDIT_BTN 0x08

// Global variable setup
// "map" between numbers and binary representation
// querying the map -> for loop (0 - 7) and the primary index is the actial number or character to display
const unsigned char MAP[10][8] = {
	{0b00000000, 0b01111110, 0b10000001, 0b10000001, 0b10000001, 0b01111110, 0b00000000, 0b00000000}, // 0
	{0b00000000, 0b00100001, 0b01000001, 0b11111111, 0b00000001, 0b00000000, 0b00000000, 0b00000000}, // 1
	{0b00000000, 0b01000011, 0b10000101, 0b10001001, 0b10010001, 0b11100001, 0b00000000, 0b00000000}, // 2
	{0b00000000, 0b10000001, 0b10001001, 0b10001001, 0b11111111, 0b00000000, 0b00000000, 0b00000000}, // 3
	{0b00000000, 0b00001100, 0b00010100, 0b00100100, 0b11111111, 0b00000100, 0b00000000, 0b00000000}, // 4
	{0b00000000, 0b11110010, 0b10010001, 0b10010001, 0b10010001, 0b10001110, 0b00000000, 0b00000000}, // 5
	{0b00000000, 0b00111110, 0b01010001, 0b10010001, 0b10010001, 0b00001110, 0b00000000, 0b00000000}, // 6
	{0b00000000, 0b10000000, 0b10001111, 0b10010000, 0b10100000, 0b11000000, 0b00000000, 0b00000000}, // 7
	{0b00000000, 0b01101110, 0b10010001, 0b10010001, 0b10010001, 0b01101110, 0b00000000, 0b00000000}, // 8
	{0b00000000, 0b01100010, 0b10010001, 0b10010001, 0b10010010, 0b01111100, 0b00000000, 0b00000000}, // 9
};

#define TICK_INTERVAL 1 // tick after one second has passed

#endif // CONFIG_H

/*
		// write data to to row register
		for (int i = 7; i >= 0; i--)
		{
			// set data for each of the six registers
			uint8_t zero = MAP[0][i] ^ 0xFF;
			uint8_t circle = MAP[1][i] ^ 0xFF;

			// write column data to appropriate row registers
			for (int j = 0; j < counter; j++)
			{
				writeToRegister(circle, j);
			}

			// activate the column register
			writeToRegister((0x01 << i) ^ 0xFF, 6);
			_delay_us(250);

			// reset the column register
			writeToRegister(0xFF, 6);
			_delay_us(1);
		}
		resetDisplay();
*/