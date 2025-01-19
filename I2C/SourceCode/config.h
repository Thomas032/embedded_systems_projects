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
#include "usart.h"
#include "bin_lib.h"

// global registers for DDR, PORT, PIN
#define I2C_CONF (DDRD)
#define I2C_OUT (PORTD)
#define I2C_IN (PIND)

// Global pin setup
#define SDA 0x01
#define SCL 0x02

// Global variable setup
#define SLAVE_ADDR 0b10100000 // EEPROM memory address
#define I2C_DELAY 5           // delay of 5 microseconds
#define MIN_ADDR 0x0000       // minimum EEPROM memory address
#define MAX_ADDR 0xFFFF       // maximum EEPROM memory address
#define MAX_CMD_LEN 50        // maximum length of command
#define WRITE_VAL 0           // the log. value of write bit for the EEPROM
#define READ_VAL 1            // the log. value of read bit for the EEPROM
#define ENTER_CODE 13         // ASCII code for enter key
#define DEBUG_OUTPUT false

#endif // CONFIG_H