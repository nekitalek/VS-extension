# VS-extension
#### Visual Studio extension written on C#
The DTE interface is used to determine statistics. First we get the code model, then we extract CodeElements from the code model. 
This object is a collection of code elements. In the collection, we look at each element, and if it is a function, we take StartPoint and EndPoint – the start and end points of the function.

At the same time, it is important to take an element of the collection as a CodeFunction, otherwise the boundaries of the functions may not be determined quite correctly. Having received such an element, we can already determine the name of the function and the number of rows in it.

The next step is to count the number of lines of code without comments. We read the entire body of the function in one line, the boundaries of the lines will be determined by the character ‘\n’. Using regular expressions, comment templates will be searched for, replaced with empty lines, and the counter responsible for the number of lines with comments will be incremented. 

Keyword search is also performed through the Regex regular expression class. An array is created with a list of all keywords, and then the Matches method of the Regex class returns the number of occurrences of this template.
