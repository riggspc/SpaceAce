#include "pch.h"
#include "GameplayView.h"
#include "vec.h"
#include "GraphicsManager.h"

#include <fstream>

static Array<byte>^ LoadShaderFile(std::string File)
{
	Array<byte>^ FileData = nullptr;

	// open the file
	std::ifstream VertexFile(File, std::ios::in | std::ios::binary | std::ios::ate);

	// if open was successful
	if (VertexFile.is_open())
	{
		// find the length of the file
		int Length = (int)VertexFile.tellg();

		// collect the file data
		FileData = ref new Array<byte>(Length);
		VertexFile.seekg(0, std::ios::beg);
		VertexFile.read(reinterpret_cast<char*>(FileData->Data), Length);
		VertexFile.close();
	}

	return FileData;
}

GameplayView::GameplayView(GraphicsManager * gm)
: View()
{

	device = gm->getDevice();
	device_context = gm->getDeviceContext();

	ComPtr<IDXGIDevice1> dxgidev;
	device.As(&dxgidev);

	ComPtr<IDXGIAdapter> dxgiAdapter;
	dxgidev->GetAdapter(&dxgiAdapter);

	ComPtr<IDXGIFactory2> dxgifactory;
	dxgiAdapter->GetParent(__uuidof(IDXGIFactory2), &dxgifactory);

	DXGI_SWAP_CHAIN_DESC1 scd = { 0 };
	scd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	scd.BufferCount = 2;
	scd.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	scd.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
	scd.SampleDesc.Count = 1;

	CoreWindow^ Window = CoreWindow::GetForCurrentThread();

	IDXGISwapChain1 ** sc_ptr = &swap_chain;

	dxgifactory->CreateSwapChainForCoreWindow(
		device.Get(),
		reinterpret_cast<IUnknown*>(Window),
		&scd,
		nullptr,
		sc_ptr);

	ComPtr<ID3D11Texture2D> backbuffer;
	swap_chain->GetBuffer(0, __uuidof(ID3D11Texture2D), &backbuffer);
	device->CreateRenderTargetView(backbuffer.Get(), nullptr, &render_target);

	D3D11_VIEWPORT viewport = { 0 };
	viewport.TopLeftX = 0;
	viewport.TopLeftY = 0;
	viewport.Width = Window->Bounds.Width;
	viewport.Height = Window->Bounds.Height;

	device_context->RSSetViewports(1, &viewport);

	InitGraphics();
	InitPipeline();
}


GameplayView::~GameplayView()
{
}

void GameplayView::draw() const{
	device_context->OMSetRenderTargets(1, render_target.GetAddressOf(), nullptr);

	float color[4] = { 0.0f, 0.2f, 0.4f, 1.0f };
	device_context->ClearRenderTargetView(render_target.Get(), color);


	UINT stride = sizeof(vec3d);
	UINT offset = 0;
	device_context->IASetVertexBuffers(0, 1, vertex_buffer.GetAddressOf(), &stride, &offset);
	device_context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	device_context->Draw(6, 0);
	device_context->Draw(3, 3);

	swap_chain->Present(1, 0);
}

void GameplayView::setElement(DrawableElement * new_elt){
	std::vector<unsigned int> old_data_idx = id_index_map.at(new_elt->getEntityId());

	std::vector<vec3d> new_data = new_elt->getData();

	auto new_iter = new_data.begin();
	auto old_iter = old_data_idx.begin();

	std::vector<unsigned int> new_indexes;
	new_indexes.reserve(new_data.size());


	// NOTE: new_iter points to a vec3d
	//       old_iter points to an index
	while (1){
		bool new_done = new_iter == new_data.end();
		bool old_done = old_iter == old_data_idx.end();

		if ( new_done && old_done ){
			// finished
			break;
		}else if (old_done){
			// more new verts than old verts, need to add new pts
			elt_ids.push_back(new_elt->getEntityId());
			elt_vertex.push_back(*new_iter);
			new_indexes.push_back(elt_vertex.size() - 1);
			++new_iter;
		}else if (new_done){
			// more old verts than new verts, need to remove pts
			elt_ids.erase(elt_ids.begin() + *old_iter);
			elt_vertex.erase(elt_vertex.begin() + *old_iter);
			++old_iter;
		}else{
			// replace an old vert with a new vert
			elt_vertex[*old_iter] = *new_iter;
			new_indexes.push_back(*old_iter);
		}
	}

	id_index_map[new_elt->getEntityId()] = new_indexes;
	de_map[new_elt->getEntityId()] = new_elt;

}

void GameplayView::removeElement(id_t associated_id){
	auto iter = de_map.find(associated_id);
	if (iter == de_map.end()) return; // not in the view

	delete iter->second;
	de_map.erase(iter);

	auto v_iter = elt_ids.begin();
	while (v_iter != elt_ids.end()){
		if (*v_iter == associated_id){
			unsigned int index = v_iter - elt_ids.begin();
			elt_vertex.erase(elt_vertex.begin() + index);
			v_iter = elt_ids.erase(v_iter);
		}else{
			++v_iter;
		}
	}

	auto iter_l = id_index_map.find(associated_id);
	if (iter_l == id_index_map.end()) return; // this should be exception, shouldn't ever happen at this point in code
	id_index_map.erase(iter_l);
}

// this function loads and initializes all graphics data
void GameplayView::InitGraphics()
{
	vec3d OurVertices[] =
	{
		{ 0.5f, 0.5f, 0.0f },
		{ -0.5f, -0.5f, 0.0f },
		{ -0.5f, 0.5f, 0.0f },
		{ 0.5f, -0.5f, 0.0f },
		{ -0.5f, -0.5f, 0.0f },
		{ 0.5f, 0.5f, 0.0f },

	};



	D3D11_BUFFER_DESC bufd = { 0 };
	bufd.ByteWidth = sizeof(vec3d)* ARRAYSIZE(OurVertices);
	bufd.BindFlags = D3D11_BIND_VERTEX_BUFFER;

	D3D11_SUBRESOURCE_DATA srd = { OurVertices, 0, 0 };

	HRESULT retv = device->CreateBuffer(&bufd, &srd, &vertex_buffer);
}

void GameplayView::InitPipeline(){
	Array<byte>^ VSFile = LoadShaderFile("VertexShader.cso");
	Array<byte>^ PSFile = LoadShaderFile("PixelShader.cso");

	device->CreateVertexShader(VSFile->Data, VSFile->Length, nullptr, &vertex_shader);
	device->CreatePixelShader(PSFile->Data, PSFile->Length, nullptr, &pixel_shader);

	device_context->VSSetShader(vertex_shader.Get(), nullptr, 0);
	device_context->PSSetShader(pixel_shader.Get(), nullptr, 0);

	D3D11_INPUT_ELEMENT_DESC ied[] =
	{
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	};

	device->CreateInputLayout(ied, ARRAYSIZE(ied), VSFile->Data, VSFile->Length, &input_layout);
	device_context->IASetInputLayout(input_layout.Get());
}