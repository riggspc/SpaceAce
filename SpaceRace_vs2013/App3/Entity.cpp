#include "pch.h"
#include "Entity.h"
#include "DrawableElement.h"

static id_t id_counter = 0;


Entity::Entity() : _id(++id_counter)
{
}

Entity::Entity(const Entity & other) : _id(other._id)
{

}

Entity::~Entity()
{
}

DrawableElement * Entity::getRenderInformation() const{
	return new DrawableElement(_id);
}

id_t Entity::getId() const{
	return _id;
}

bool Entity::matchesId(const Entity * other) const{
	return _id == other->_id;
}