#pragma once

using namespace Microsoft::WRL;
using namespace Windows::UI::Core;
using namespace Platform;
using namespace DirectX;

// this class SHOULD be abstract but i doubt we're going to use anything but directx 
// and it would be nontrivial to provide a framework independent ABC so to hell with that

// this class's responsibilities include providing an "interface" to the system's graphics framework

// really it just knows how to allocate the device and device context, which is the important part for now

class GraphicsManager 
{
public:
	GraphicsManager();
	virtual ~GraphicsManager();

	ComPtr<ID3D11Device1> getDevice();
	ComPtr<ID3D11DeviceContext1> getDeviceContext();

private:

	ComPtr<ID3D11Device1> device;
	ComPtr<ID3D11DeviceContext1> device_context;

};

