#include "pch.h"
#include "Game.h"


Game::Game()
{

}


Game::~Game()
{
	for (auto pair : entities){
		if(pair.second) delete pair.second;
	}
}

void Game::step(double delta_time){
	
}

Entity * Game::popEntity(id_t entity_id){
	auto iter = entities.find(entity_id);

	if (iter == entities.end()) return nullptr;

	Entity * retv = iter->second;
	entities.erase(iter);
	return retv;
}

void Game::pushEntity(Entity * entity){
	entities[entity->getId()] = entity;
}

const Entity const * Game::getEntity(id_t entity_id) const{
	auto iter = entities.find(entity_id);

	if (iter == entities.end()) return nullptr;

	const Entity const * retv = iter->second;
	return retv;
}