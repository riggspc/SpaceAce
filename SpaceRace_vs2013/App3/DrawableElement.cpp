#include "pch.h"
#include "DrawableElement.h"


DrawableElement::DrawableElement()
{
}


DrawableElement::~DrawableElement()
{
}

bool DrawableElement::hasCommonEntity(const DrawableElement * other) const{
	return associated_entity_id == other->associated_entity_id;
}