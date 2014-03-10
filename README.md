![alt tag](https://raw.github.com/Thodd/lgRuntime/master/img/logo_lemongrasp.png)
==========

The `LemonGrasp` runtime for Windows. 
LemonGrasp is an interaction prototyping tool, which allows you to define certain qualities of multi-touch gestures.
It's part of the German research project ProTACT (http://www.pro-tact.de) and ultimately aims to address the more dynamic aspects of Touch-Prototyping.

### What can you currently do?
The current version of LemonGrasp allows you to fiddle with the interaction-attributes, inherent to the three most common multi-touch gestures: `Move`, `Rotate` and `Scale`:

![alt tag](https://raw.github.com/Thodd/lgRuntime/master/img/manipulations.png)

Possible attribute changes are for example the `speed` of a gesture, or the possibility of moving an object in a `discrete` fashion.

### Technical Stuff
LemonGrasp has a very basic architecture, as depicted in the image below:

![alt tag](https://raw.github.com/Thodd/lgRuntime/master/img/architecture.png)

The final version of LemonGrasp will save the prototype in JSON format. The lgRuntime (this github project) will then interprete the JSON data and run the prototype in fullscreen.

This github project is an implementation of the LemonGrasp Runtime for `Windows 7 and 8`.
Additional runtimes for iOS devices and a HTML5/JavaScript version is also planned. Please feel free to contribute to this goal by re-implementing the lgRuntime (Windows) on other platforms.
JSON was chosen as an intermediate data transfer format to allow an easier integration into Webapps.

### Publications on LemonGrasp
LemonGrasp: a tool for touch-interaction prototyping

```
Thorsten Hochreuter. 2014. LemonGrasp: a tool for touch-interaction prototyping. 
In Proceedings of the 8th International Conference on Tangible, Embedded and Embodied Interaction (TEI '14). 
ACM, New York, NY, USA, 317-320. 
DOI=10.1145/2540930.2558130 
http://doi.acm.org/10.1145/2540930.2558130
```

Paper PDF can be found in the ACM Digital Library:

http://dl.acm.org/citation.cfm?id=2558130
