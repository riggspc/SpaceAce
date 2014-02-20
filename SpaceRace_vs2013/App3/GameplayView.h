#pragma once

#include "View.h"
#include "DrawableElement.h"

#include <vector>
#include <map>

using namespace Microsoft::WRL;
using namespace Windows::UI::Core;
using namespace Platform;
using namespace DirectX;

class GraphicsManager;

class GameplayView :
	public View
{
public:
	GameplayView(GraphicsManager * gm);
	~GameplayView();

	virtual void draw() const; // TODO

	// adds/modifies element data to hold vertexs for this drawablelement
	//note: gives ownership to the view
	virtual void setElement(DrawableElement * new_elt);
	virtual void removeElement(id_t associated_id); // TODO

private:
	// list of all vertexes this view knows about
	std::vector<id_t> elt_ids;     // contains entity id of corresponding vertex
	std::vector<vec3d> elt_vertex; // contains vec3d representing vertex

	// maps entity id to a vector containing all indexs for that id (see above vectors)
	std::map<id_t, std::vector<unsigned int>> id_index_map;
	std::map<id_t, DrawableElement*> de_map;

	ComPtr<ID3D11Device1> device;
	ComPtr<ID3D11DeviceContext1> device_context;
	ComPtr<IDXGISwapChain1> swap_chain;
	ComPtr<ID3D11RenderTargetView> render_target;
	ComPtr<ID3D11Buffer> vertex_buffer;

	ComPtr<ID3D11VertexShader> vertex_shader;
	ComPtr<ID3D11PixelShader> pixel_shader;

	ComPtr<ID3D11InputLayout> input_layout;

	void InitGraphics();
	void InitPipeline();
};

