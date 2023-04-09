
#include <Arduino.h>
#include "Adafruit_VL53L0X.h"

#define LED_RED 2
#define CENTER_BUTTON 4
#define SAFETY_BUTTON 5

Adafruit_VL53L0X lox = Adafruit_VL53L0X();

int distance = 0;
int pressed;
int center_dist = 0;
int alert_dist = -1;
int safety_hits = 0;
int risk = 0;
int alert = 0;
bool warn = false;

void setup() {
  Serial.begin(9600);
  pinMode(CENTER_BUTTON, INPUT_PULLUP);
  pinMode(SAFETY_BUTTON, INPUT_PULLUP);
  pinMode(LED_RED, OUTPUT);

  // wait until serial port opens for native USB devices
  while (! Serial) {
    delay(1);
  }

  //Serial.println("Adafruit VL53L0X test.");
  if (!lox.begin()) {
    Serial.println(F("Failed to boot VL53L0X"));
    while(1);
  }
  // power
  //Serial.println(F("VL53L0X API Continuous Ranging example\n\n"));

  // start continuous ranging
  lox.startRangeContinuous();
} 

void checkSafetyButton() {
  if (!digitalRead(SAFETY_BUTTON)) {
    alert = false;
    warn = false;
  }
}

void loop() {
  pressed = !digitalRead(CENTER_BUTTON);
  if (lox.isRangeComplete()) {
    distance = lox.readRange();
    // Serial.print("Distance in mm: ");
    // Serial.println(distance);
  }
  if (pressed) {
    center_dist = distance;
    alert_dist = .765 * center_dist;
    Serial.print("Center set to: ");
    Serial.println(center_dist);
    delay(200);
  }

  checkSafetyButton();
  // Enable led based on alert status:
  digitalWrite(LED_RED, alert);

  // Check if distance dropped significantly to imply a head nod:
  if (distance <= (alert_dist) && !warn) {
    Serial.println("WAKE UP");
    risk += 1;
    Serial.printf("RISK: %i\n", risk);
    warn = true;
    alert = 1;
    delay(100);
  }
}