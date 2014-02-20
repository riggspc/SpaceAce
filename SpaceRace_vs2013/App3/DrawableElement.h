#pragma once

// A DrawableElement is a unique
// element containing an id and just
// enough information for a View to render
// it onscreen.

#include "Entity.h"

// TODO: add render data, duh

class DrawableElement
{
public:
	DrawableElement(id_t id); // standard ctor generates an element that is invisible upon rendering
	~DrawableElement();

	// Checks to see if both drawable elements refer to the same
	// entity, by matching ID.
	bool hasCommonEntity(const DrawableElement * other) const;

	id_t getEntityId() const;

	bool operator<(const DrawableElement & rhs) const;
	bool operator==(const DrawableElement & rhs) const;

private:

	id_t associated_entity_id;
};

bool cmp_drawable_elt_ptr_lt(const DrawableElement * lhs, const DrawableElement * rhs);

class cmp_de_ptr_lt_ftor{
public:
	bool operator()(const DrawableElement * lhs, const DrawableElement * rhs){
		return cmp_drawable_elt_ptr_lt(lhs, rhs);
	}
};