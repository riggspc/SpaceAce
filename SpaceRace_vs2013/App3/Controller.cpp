#include "pch.h"
#include "Controller.h"
#include "Game.h"
#include "GameplayView.h"


Controller::Controller()
: game_state(new Game),
  display(new GameplayView)
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