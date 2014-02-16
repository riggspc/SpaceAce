#include "pch.h"
#include "CGame.h"

#include <string>
#include <fstream>
	


Array<byte>^ LoadShaderFile(std::string File)
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


void CGame::Initialize(){

	ComPtr<ID3D11Device> tmp_dev;
	ComPtr<ID3D11DeviceContext> tmp_con;

	D3D11CreateDevice(
		nullptr,
		D3D_DRIVER_TYPE_HARDWARE,
		nullptr,
		0,
		nullptr,
		0,
		D3D11_SDK_VERSION,
		&tmp_dev,
		nullptr,
		&tmp_con
		);

	tmp_dev.As(&device);
	tmp_con.As(&device_context);

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

// this function loads and initializes all graphics data
void CGame::InitGraphics()
{
	VERTEX OurVertices[] =
	{
		{ 0.5f, 0.5f, 0.0f },
		{ -0.5f, -0.5f, 0.0f },
		{ -0.5f, 0.5f, 0.0f },
		{ 0.5f, -0.5f, 0.0f },
		{ -0.5f, -0.5f, 0.0f },
		{ 0.5f, 0.5f, 0.0f },
		
	};

	

	D3D11_BUFFER_DESC bufd = { 0 };
	bufd.ByteWidth = sizeof(VERTEX)* ARRAYSIZE(OurVertices);
	bufd.BindFlags = D3D11_BIND_VERTEX_BUFFER;

	D3D11_SUBRESOURCE_DATA srd = { OurVertices, 0, 0 };

	HRESULT retv = device->CreateBuffer(&bufd, &srd, &vertex_buffer);

	D3D11_TEXTURE2D_DESC depthStencilDesc;

	depthStencilDesc.Width = Width;
	depthStencilDesc.Height = Height;
	depthStencilDesc.MipLevels = 1;
	depthStencilDesc.ArraySize = 1;
	depthStencilDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
	depthStencilDesc.SampleDesc.Count = 1;
	depthStencilDesc.SampleDesc.Quality = 0;
	depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
	depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
	depthStencilDesc.CPUAccessFlags = 0;
	depthStencilDesc.MiscFlags = 0;
}

void CGame::InitPipeline(){
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

void CGame::Update(){

}

void CGame::Render(){
	device_context->OMSetRenderTargets(1, render_target.GetAddressOf(), nullptr);
	
	float color[4] = { 0.0f, 0.2f, 0.4f, 1.0f };
	device_context->ClearRenderTargetView(render_target.Get(), color);
	

	UINT stride = sizeof(VERTEX);
	UINT offset = 0;
	device_context->IASetVertexBuffers(0, 1, vertex_buffer.GetAddressOf(), &stride, &offset);
	device_context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	device_context->Draw(6, 0);
	device_context->Draw(3, 3);

	swap_chain->Present(1, 0);
}

void CGame::Close(){

}