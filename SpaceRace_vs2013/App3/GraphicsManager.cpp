#include "pch.h"
#include "GraphicsManager.h"


GraphicsManager::GraphicsManager()
{
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
}


GraphicsManager::~GraphicsManager()
{
}

ComPtr<ID3D11Device1> GraphicsManager::getDevice(){
	return device;
}

ComPtr<ID3D11DeviceContext1> GraphicsManager::getDeviceContext(){
	return device_context;
}