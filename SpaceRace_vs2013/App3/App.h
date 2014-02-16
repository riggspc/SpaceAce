#pragma once

#include "pch.h"
#include "CGame.h"

// Use some common namespaces to simplify the code
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::ApplicationModel::Activation;
using namespace Windows::UI::Core;
using namespace Windows::UI::Popups;
using namespace Windows::System;
using namespace Windows::Foundation;
using namespace Windows::Graphics::Display;
using namespace Platform;


// callback house, essentially
ref class App sealed : public IFrameworkView{
public:
	
	virtual void Initialize(CoreApplicationView^ AppView);
	virtual void SetWindow(CoreWindow^ Window);
	virtual void Load(String^ EntryPoint);
	virtual void Run();
	virtual void Uninitialize();
	
	void OnActiviated(CoreApplicationView^ CoreAppView, IActivatedEventArgs^ Args);
	void Closed(CoreWindow^ sender, CoreWindowEventArgs^ args);
	void Suspending(Object^ Sender, SuspendingEventArgs^ Args);
	void Resuming(Object^ Sender, Object^ Args);

private:
	bool window_closed;
	CGame Game;
};


// app factory
ref class AppSource sealed : IFrameworkViewSource{
public:
	virtual IFrameworkView^ CreateView();
		
};