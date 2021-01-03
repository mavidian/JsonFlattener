# JsonFlattener
A class library to convert JSON data to/from a flattened set of key value pairs.

## Installation

* Fork this repository and clone it onto your local machine, or

* Download this repository onto your local machine.

## JSON Flattening Convention

By definition, a key-value pair (KVP) is flat or 2-dimensional; the dimiensions are the key and the value.

JSON on the other hand is hierarchical or multi-dimensional. Every JSON an object can contain inner objects. There is no limit on the JSON nesting hierarchy.

To facilitate translation between hierarchical JSON and flat KVP, the KVP's key must reflect
the hierarchy of the corresponding JSON value. JsonFlattener uses the [Path](https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonReader_Path.htm){:target="_blank"}
property of the Newtonsoft's JsonReader (or JsonWriter) object for this purpose.

The KVP's key consists of period (.) separated segment names, where each name reflects the nesting level
in the corresponding JSON hierarchy. Each level, except for the top (last), can be an inner object or an array element. In case of an inner object, a JSON
property name is used. In case of an array element, a property name is followed by a 0-based index inside square brackets. The top (last) segment
simply contains the property name (posssibly with an index if an array element).

As an extension of this convension, we can think of a set of key-value pairs as being 3-dimensional.
The JSON equivalent is a JSON array where each element is a JSON object.

Examples:

<table>
<tr>
<td> <b>JSON</b> </td> <td><b>KVP</b> </td>
</tr>
<tr>
<td>

```json
{
   "name": "John",
   "age": "30",
   "city": "Chicago"
}
```
</td>
<td>

```
Key=name  Value=John
Key=age   Value=30
Key=city  Value=Chicago
```
</td>
</tr><tr>
<td>

```json
[
  "John",
  "Susan",
  "Bob"
]
```

</td>
<td>

```
Key=[0]  Value=John
Key=[1]  Value=Susan
Key=[2]  Value=Bob
```
</td>
</tr><tr>
<td>

```json
[
  { "name":"John","age":"30" },
  { "name":"Susan","age":"24" },
  {
    "name":"Joe","age":"42",
    "kids":["Amy","Kate","Jim"]
  }
]
```
</td>
<td>

```
Key=[0].name  Value=John
Key=[0].age  Value=30
Key=[1].name  Value=Susan
Key=[1].age  Value=24
Key=[2].name  Value=Joe
Key=[2].age  Value=42
Key=[2].kids[0]  Value=Amy
Key=[2].kids[1]  Value=Kate
Key=[2].kids[2]  Value=Jim
```

</td>
</tr>
</table>


## Caveats

* Empty JSON objects or arrays do not get reflected in the resulting set of KVPs.
 
    Examples:

```json
    { "EmptyArray": [] }
    { "ArrayWithEmptyObject": [ {} ] }
    { "EmptyObject": {} }
    { "ObjectWithEmptyArray": { "EmptyArray": [] } }
```

* Since underscore (_) is used to separate key segments, JSON fields with names containing it will become nested objects upon round trip translation.

    For example,
```json
    { "my_field": "my_value" }
```
    will become this KVP:
```text
    Key=my_field  Value=my_value
```
    , which in turn will be translated to:
```json
{ "my": { "field": "my_value" } }
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

## Copyright

```
Copyright © 2020 Mavidian Technologies Limited Liability Company. All Rights Reserved.
```