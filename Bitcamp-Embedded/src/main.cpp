
#include <Arduino.h>
#include "Adafruit_VL53L0X.h"

#define CENTER_BUTTON 4
#define SAFETY_BUTTON 5

Adafruit_VL53L0X lox = Adafruit_VL53L0X();

int distance = 0;
int pressed;
int center_dist = 0;
int safety_hits = 0;

void setup() {
  Serial.begin(9600);
  pinMode(CENTER_BUTTON, INPUT_PULLUP);
  pinMode(SAFETY_BUTTON, INPUT_PULLUP);
  pinMode(LED_BUILTIN, OUTPUT);

  // wait until serial port opens for native USB devices
  while (! Serial) {
    delay(1);
  }

  Serial.println("Adafruit VL53L0X test.");
  if (!lox.begin()) {
    Serial.println(F("Failed to boot VL53L0X"));
    while(1);
  }
  // power
  Serial.println(F("VL53L0X API Continuous Ranging example\n\n"));

  // start continuous ranging
  lox.startRangeContinuous();
} 

void avg() {

}

void loop() {
  pressed = !digitalRead(CENTER_BUTTON);
  pressed = !digitalRead(SAFETY_BUTTON);
  digitalWrite(LED_BUILTIN, pressed);
  if (lox.isRangeComplete()) {
    distance = lox.readRange();
    // Serial.print("Distance in mm: ");
    // Serial.println(distance);
  }
  if (pressed) {
    center_dist = distance;
    Serial.print("Center set to: ");
    Serial.println(center_dist);
    delay(200);
  }
  if (distance <= (center_dist-50)) {
    Serial.println("WAKE UP!");
    delay(100);
  }
}