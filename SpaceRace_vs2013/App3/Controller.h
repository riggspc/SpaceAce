#pragma once

class Game;
class View;

class Controller
{
public:
	Controller();

	// runs game until game is finished
	void run();

private:
	Game * game_state;
	View * display;
};

