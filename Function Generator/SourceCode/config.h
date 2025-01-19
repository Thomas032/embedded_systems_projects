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

// Global register setup
#define CH1_CONF (DDRC)
#define CH2_CONF (DDRA)

#define CH1_OUT (PORTC)
#define CH2_OUT (PORTA)


// Global variable setup
#define DIGITAL_MAX_AMP 210
#define ANALOG_MAX_AMP 4200
#define SAMPLING_RATE 360 // always an even number
#define MAX_CMD_LEN 50

#endif // CONFIG_H