#include "pch.h"
#include "Controller.h"
#include "Game.h"
#include "GameplayView.h"
#include "GraphicsManager.h"


Controller::Controller()
: game_state(new Game),
  graphics_manager(new GraphicsManager),
  display(new GameplayView(graphics_manager))
{
}

Controller::~Controller()
{
	delete game_state;
	delete display;
}

void Controller::run_step(){
	display->draw();
}