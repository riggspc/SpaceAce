#ifndef VIEW_H
#define VIEW_H

class DrawableElement;

class View{
public:

	View();
	virtual ~View();

	virtual void draw() const = 0;
	virtual void setElement(const DrawableElement * new_elt) = 0;
	virtual void removeElement(const DrawableElement * new_elt) = 0;

private:

};


#endif