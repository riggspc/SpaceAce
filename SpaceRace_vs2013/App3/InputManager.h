#pragma once

class InputSignal;

#include <vector>

// this class is responsible for polling up all inputs
// into discrete packages for the Controller to interpret

class InputManager{
public:
	InputManager();
	virtual ~InputManager();

	// polls all input and packages it into our
	// buffer
	virtual void poll(); // TODO

	// takes ownership of all input signals in the buffer
	std::vector<InputSignal*> fetchAllInput(); // TODO

private:
	std::vector<InputSignal*> inputs;
};

