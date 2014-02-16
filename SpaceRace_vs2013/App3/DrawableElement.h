#pragma once

// A DrawableElement is a unique
// element containing an id and just
// enough information for a View to render
// it onscreen.

class DrawableElement
{
public:
	DrawableElement();
	~DrawableElement();

	// Checks to see if both drawable elements refer to the same
	// entity, by matching ID.
	bool hasCommonEntity(const DrawableElement * other) const;

private:

	unsigned int associated_entity_id;
};

