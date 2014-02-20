#pragma once

class Game;
class View;
class GraphicsManager;

class Controller
{
public:
	Controller();
	virtual ~Controller();

	// runs game until game is finished
	void run_step();

private:
	Game * game_state;
	GraphicsManager * graphics_manager;
	View * display;

};

