#include "pch.h"

#include "App.h"

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

#include <iostream>

[MTAThread]

int main(Array<String^>^ args){
	std::cout << "asdasd" << std::endl;

	CoreApplication::Run(ref new AppSource());

	return 0;
}