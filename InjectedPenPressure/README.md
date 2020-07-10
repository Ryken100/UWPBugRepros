# Injected Pen Pressure Bug
When using `Windows.Input.Preview.Injection.InputInjector` to inject pen inputs, the actual injected pen pressure will be one quarter of the value set on `InjectedInputPenInfo.Pressure`. So if `InjectedInputPenInfo.Pressure` is set to 1, the APIs will inject a pen input with a pressure of 0.25. If it is set to 0.5, the APIs will inject a pen input with a pressure of 0.125.

Setting `InjectedInputPenInfo.Pressure` to a value greater than 1 will always generate an exception, so you cannot set it to 4 and get an injected input with a pressure of 1.

In this repro, inputs are injected in the `ProcessTopGridInput` and `manualInputButton_Click` methods in MainPage.xaml.cs.
