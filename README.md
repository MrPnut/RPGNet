# RPGNet
(**This is currently back into development**).

RPGLE Compiler for .NET.

Since this is a free-time-project only, it won't be worked on as much as I like. The idea behind this project is to create a compiler based on free-format RPG which would compile down to IL/CIL/MSIL/.NET. I want users to be able to copy their source code and compile it with minimal change.

RPGNet is actually in a decent state at the moment. A current list of features would include (in no particular order):
* Build-In Functions (not all of them)
* Variables
* Procedures
* Global/Local variables
* Arrays
* Totally free-format
* Error catching (Monitoring)
* Embedded SQL (not commited, as a work in progress)

Operation codes including:
* IF
* ELSEIF
* SELECT / WHEN
* DSPLY
* MONITOR / ON-ERROR / ENDMON
* DOW
* WAIT (Just for testing, really)
* EXEC SQL

Some code examples:

```
Dcl-Proc Select;

  Dcl-S Number Int;
  Number = 6;
  
  Select;
  
    When Number = 5;
      Dsply 'IT''S 5!!';
      
    When Number = 6;
      If 5 <> 6;
        Dsply '5 is not 6';
      Endif;
      Dsply 'IT''S 6!!';
      
  Endsel;

  Dsply 'yes';
  Wait;

End-Proc;
```

```
Dcl-Proc RealSpacingFix;

  Dcl-S Text Varchar;
  Dcl-S theScan Int;

  Text = 'Hello world';
  theScan = %Scan(' ':Text);
  Dsply 'Scan: ' + %Char(theScan);

  Wait;

End-Proc;
```

```
Dcl-Proc MonTest;

  Monitor;
    Dsply %Int('18.5');
  On-Error;
    Dsply 'NO';
  Endmon;
  Wait;

End-Proc;
```
