#include "pch.h"

#include "App.h"
#include "Controller.h"

void App::Initialize(CoreApplicationView^ AppView){
	AppView->Activated += ref new TypedEventHandler<CoreApplicationView^, IActivatedEventArgs^>(this, &App::OnActiviated);
	CoreApplication::Suspending += ref new EventHandler<SuspendingEventArgs^>(this, &App::Suspending);
	CoreApplication::Resuming += ref new EventHandler<Object^>(this, &App::Resuming);

	control = nullptr;

	window_closed = false;
}

void App::SetWindow(CoreWindow^ Window){
	Window->Closed += ref new TypedEventHandler<CoreWindow^, CoreWindowEventArgs^>(this, &App::Closed);
}

void App::Load(String^ EntryPoint){
	// do nothing
}

void App::Run(){

	if (!control) control = new Controller();

	CoreWindow^ Window = CoreWindow::GetForCurrentThread();

	while (!window_closed){
		Window->Dispatcher->ProcessEvents(CoreProcessEventsOption::ProcessAllIfPresent);

		control->run_step();
		// blah
	}
}

void App::Uninitialize(){
	// nothing to do
	delete control;
}

void App::OnActiviated(CoreApplicationView^ CoreAppView, IActivatedEventArgs^ Args){
	CoreWindow^ Window = CoreWindow::GetForCurrentThread();
	Window->Activate();
}

void App::Closed(CoreWindow^ sender, CoreWindowEventArgs^ args){
	window_closed = true;
}

void App::Suspending(Object^ sender, SuspendingEventArgs^ Args){
	// do nothing
}

void App::Resuming(Object^ Sender, Object^ Args){
	// do nothing
}

IFrameworkView^ AppSource::CreateView(){
	return ref new App;
}
