#pragma once

class DrawableElement;

// Entity is the baseclass for all SpaceAce game agents.
// It provides base functionality such as an agent ID, and
// probably other stuff eventually

typedef unsigned int id_t;

class Entity{
public:
	Entity();
	Entity(const Entity & other);
	virtual ~Entity();

	// makes the DrawableElement to be passed to the view.
	virtual DrawableElement * getRenderInformation() const;

	// id, to identify particular objects
	id_t getId() const;
	bool matchesId(const Entity * other) const;

protected:



private:
	id_t _id;
	
};

