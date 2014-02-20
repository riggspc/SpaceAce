#pragma once

// need to get id_t
#include "Entity.h"

#include <map>

// Base class for game operation.
// This class contains all information about the game state

class Game{
public:
	Game();
	virtual ~Game();

	// Process delta_time seconds worth of gameplay
	// given current known state
	virtual void step(double delta_time); // TODO

	// gets entity for further processing, effectively
	// cedeing ownership. Returns nullptr if no matching entity exists
	Entity * popEntity(id_t entity_id);

	// Gives entity to the game for control, gives up ownership
	void pushEntity(Entity * entity);

	// gets a const reference to an entity for non-mutable use.
	// does not change ownership. nullptr if no matching entity exists
	const Entity const * getEntity(id_t entity_id) const;

protected:

private:
	std::map<id_t, Entity*> entities;

};

