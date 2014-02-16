#pragma once

using namespace Microsoft::WRL;
using namespace Windows::UI::Core;
using namespace Platform;
using namespace DirectX;

struct VERTEX{
	float x, y, z;
};

class CGame sealed
{
public:

	void Initialize();
	void InitGraphics();
	void InitPipeline();
	void Update();
	void Render();
	void Close();

private:

	ComPtr<ID3D11Device1> device;
	ComPtr<ID3D11DeviceContext1> device_context;
	ComPtr<IDXGISwapChain1> swap_chain;
	ComPtr<ID3D11RenderTargetView> render_target;
	ComPtr<ID3D11Buffer> vertex_buffer;

	ComPtr<ID3D11VertexShader> vertex_shader;
	ComPtr<ID3D11PixelShader> pixel_shader;

	ComPtr<ID3D11InputLayout> input_layout;

	ComPtr<ID3D11Texture2D> depth_buffer;
	ComPtr<ID3D11DepthStencilView> depth_stencil;

};

