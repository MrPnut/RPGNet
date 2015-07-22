# RPGNet
RPGLE Compiler for .NET.

Since this is a free-time-project only, it won't be worked on as much as I like. The idea behind this project is to create a compiler based on free-format RPG which would compile down to IL/CIL/MSIL/.NET. I want users to be able to copy their source code and compile it with minimal change.

Obviously there has to be things that have to be changed to suit the .NET framework. For example, file declarations or the DSPLY operation codes, as well as some other things. The type system would obviosuly have to be different from the iSeries version. For example, I plan to keep VARCHAR and alias CHAR to VARCHAR too. Int will remain the same. For example you can define different INT types (6, 16 or 32 bit) as well as different PACKED types (which would be FLOAT within the CIL).
