#ifndef VIEW_H
#define VIEW_H

class DrawableElement;
#include "Entity.h"

class View{
public:

	virtual ~View(){}

	virtual void draw() const = 0;
	virtual void setElement(DrawableElement * new_elt) = 0;
	virtual void removeElement(id_t associated_id) = 0;

private:

};


#endif