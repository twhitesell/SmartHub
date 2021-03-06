#include <MySensor.h>
#include <SPI.h>
//--------------------------------------------------------------------------------------------------------------------------------------------
#define NUMBER_OF_RELAYS		8  // Total number of attached relays
//uint8_t pins[NUMBER_OF_RELAYS] = { A0, A1, A2, A3, A4, A5, 4, 2 }; // A6 and A7 don't support digitalWrite!!!! (for my PCB)
uint8_t pins[NUMBER_OF_RELAYS] = { 2, 3, 4, 5, 6, 7, 8, A0 }; // A6 and A7 don't support digitalWrite!!!! (for Nano IO Shield)
#define RELAY_ON				0  // GPIO value to write to turn on attached relay
#define RELAY_OFF				1  // GPIO value to write to turn off attached relay
MyMessage msgRelay(0, V_LIGHT);
MySensor gw(DEFAULT_CE_PIN, DEFAULT_CS_PIN);
//--------------------------------------------------------------------------------------------------------------------------------------------
void setup()
{
	gw.begin(onMessageReceived);
	gw.sendSketchInfo("Power switch 8", "1.0");

	// (sensorID = 0...7)
	for (int sensorID = 0; sensorID < NUMBER_OF_RELAYS; sensorID++)
	{
		uint8_t pin = pins[sensorID];
		pinMode(pin, OUTPUT);
		uint8_t lastState = gw.loadState(sensorID);
		digitalWrite(pin, lastState ? RELAY_ON : RELAY_OFF);

		gw.present(sensorID, S_LIGHT);
		gw.send(msgRelay.setSensor(sensorID).set(lastState ? 1 : 0));
	}
}
void loop()
{
	gw.process();
}

void onMessageReceived(const MyMessage &message)
{
	uint8_t cmd = mGetCommand(message);

	if (message.type == V_LIGHT)
	{
		uint8_t lastState = gw.loadState(message.sensor);

		if (cmd == C_SET)
		{
			uint8_t newState = message.getByte();
			if (newState != lastState)
			{
				digitalWrite(pins[message.sensor], newState ? RELAY_ON : RELAY_OFF);
				gw.saveState(message.sensor, newState);

				gw.send(msgRelay.setSensor(message.sensor).set(newState));
			}
		}
		else if (cmd == C_REQ)
			gw.send(msgRelay.setSensor(message.sensor).set(lastState));
	}
}
