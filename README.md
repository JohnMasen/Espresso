Espresso / Espresso-VE / Espresso-ND
===============

V8 js engine with C# (in-process), => **Espresso-VE**

NodeJS engine with C# (in-process), => **Espresso-ND**

---

**Espresso** (from [vroomjs](https://github.com/Daniel15/vroomjs) ) is a bridge between the .NET CLR (think C# or F#) and the V8 Javascript
engine that uses P/Invoke and a thin C layer (_libespr_).

Now, **Espresso can run on .net20+ and  .netcore/.netstandard**

so We can run the engine on **Windows7+, macOS, and Linux(tested with Ubuntu 16)**

---

**Windows7**

![11](https://user-images.githubusercontent.com/9301044/26943986-96459b9c-4cb1-11e7-9c26-25b7d232f375.png)

![12](https://user-images.githubusercontent.com/9301044/26944008-a83905a0-4cb1-11e7-808e-5769517d44bc.png)

![13](https://user-images.githubusercontent.com/9301044/26944012-aab0061c-4cb1-11e7-9a23-7e99094b0dd3.png) 

---

**macOS, x64**


![26](https://user-images.githubusercontent.com/9301044/26942872-da890496-4cad-11e7-915f-30a24caef5f3.png)

![27](https://user-images.githubusercontent.com/9301044/26942918-021a1ffe-4cae-11e7-8d5d-6ab38a2857be.png)
  

---

**Linux, Ubuntu 16, x64**

![20](https://user-images.githubusercontent.com/9301044/26941879-4ac77e9e-4caa-11e7-823a-bdbb6b629842.png) 

![21](https://user-images.githubusercontent.com/9301044/26941920-6edb05e4-4caa-11e7-92b1-ed907b837acd.png)

![23](https://user-images.githubusercontent.com/9301044/26942079-f2f6afcc-4caa-11e7-9c85-470436e072c2.png)

---
With Espresso it is possible to execute arbitrary javascript code and get the
result as a managed primitive type (for integers, numbers, strings, dates and
arrays of primitive types) or as a `JsObject` wrapper that allows to
dynamically access properties and call functions on Javascript objects.

Each `JsEngine` is an isolated V8 context and all objects allocated on the
Javascript side are persistent over multiple calls. It is possible to set and
get global variables. Variable values can be primitive types, CLR objects or
`JsObjects` wrapping Javascript objects. CLR instances are kept alive as long
as used in Javascript code (so it isn't required to track them in client code:
they won't be garbage collected as long as references on the V8 side) and it is
possible to access their properties and call methods from JS code.

**Current V8 Engine => We use V8 from node v7.10.0**

Examples
--------

Execute some Javascript:
```C#
	using (var js = new JsEngine()) {
		var x = (int)js.Execute("3.14159+2.71828");
		Console.WriteLine(x);  // prints 5.85987
	}
```
 

Create and return a Javascript object, then call a method on it:

```C#
	using (var js = new JsEngine()) {
		// Create a global variable on the JS side.
		js.Execute("var x = {'answer':42, 'tellme':function (x) { return x+' '+this.answer; }}");
		// Get it and use "dynamic" to tell the compiler to use runtime binding.
		dynamic x = js.GetVariable("x");
		// Call the method and print the result. This will print:
		// "What is the answer to ...? 42"
		Console.WriteLine(x.tellme("What is the answer to ...?"));
	}
```
Access properties and call methods on CLR objects from Javascript:

```C#
	class Test
	{
		public int Value { get; set; }
		public void PrintValue(string msg)
		{
			Console.WriteLine(msg+" "+Value);
		}
	}
	
	using (var js = new JsEngine()) {
		js.SetVariable("m", new Test());
		// Sets the property from Javascript.
		js.Execute("m.Value = 42");
		// Call a method on the CLR object from Javascript. This prints:
		// "And the answer is (again!): 42"
		js.Execute("m.PrintValue('And the answer is (again!):')");
	}
```

---------------

**Espresso-ND** 

Espresso-ND is special edition of the Espresso, 
It is NodeJS in dll form + Espresso Bridge code,

so you can run NodeJS app in-process with .NET Code

see example, run nodejs http server 
![slide18](https://cloud.githubusercontent.com/assets/9301044/21955739/95196ecc-daa3-11e6-8e91-c0f443ac0165.png)
![slide19](https://cloud.githubusercontent.com/assets/9301044/21955741/951b7442-daa3-11e6-932f-a76dbeb9358d.PNG)



see how to build it at https://github.com/prepare/Espresso/issues/30




 
