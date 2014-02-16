#pragma once

// need to get id_t
#include "Entity.h"

// Base class for game operation.
// This class contains all information about the game state

class Game{
public:
	Game();
	~Game();

	// Process delta_time seconds worth of gameplay
	// given current known state
	virtual void step(double delta_time);

	// gets entity for further processing, effectively
	// cedeing ownership. Returns null if no matching entity exists
	Entity * popEntity(id_t entity_id);

	// Gives entity to the game for control, gives up ownership
	void pushEntity(const Entity * entity);

	// gets a const reference to an entity for non-mutable use.
	// does not change ownership.
	const Entity const * getEntity(id_t entity_id) const;

protected:

private:


};

