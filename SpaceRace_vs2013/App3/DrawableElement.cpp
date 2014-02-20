#include "pch.h"
#include "DrawableElement.h"


DrawableElement::DrawableElement(id_t id)
: associated_entity_id(id)
{
}


DrawableElement::~DrawableElement()
{
}

bool DrawableElement::hasCommonEntity(const DrawableElement * other) const{
	return associated_entity_id == other->associated_entity_id;
}

id_t DrawableElement::getEntityId() const{
	return associated_entity_id;
}

bool DrawableElement::operator<(const DrawableElement & rhs) const{
	return associated_entity_id < rhs.associated_entity_id;
}

bool DrawableElement::operator==(const DrawableElement & rhs) const{
	return hasCommonEntity(&rhs);
}

bool cmp_drawable_elt_ptr_lt(const DrawableElement * lhs, const DrawableElement * rhs){
	return *lhs < *rhs;
}