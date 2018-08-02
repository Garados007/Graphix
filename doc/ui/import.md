# UI Language - Imports

To make the file size smaller and more readable the complete definition can split among 
many files. Theirfore is the import command in the root of the definition:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Imports>
    <File name="other-file.xml" />
  </Imports>
</Objects>
```

If the parser reach this position, the parsing process halts and the file is readed. Its data is
then added to the current data list. Normaly the called file has access to all objects that was
definied before the call.

## multiple files

Its very easy to import multiple files at the same time. The order of precense in the list is
the order of loading this files.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Imports>
    <File name="other-file1.xml" />
    <File name="other-file2.xml" />
    <File name="other-file3.xml" />
    <File name="other-file4.xml" />
    <File name="other-file5.xml" />
  </Imports>
</Objects>
```

## required files

Normaly each import file is optional. If you want to require some files then the definition 
has to be extend:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Imports>
    <File name="other-file.xml" require="true" />
  </Imports>
</Objects>
```

## filter import

If you want to import specific objects you can define it in the definition by adding the filter:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Imports>
    <File name="other-file.xml" require="true">
	  <IncludeStat name="ImportantStatus" />
	  <IncludeProp name="ImportantPrototype" />
	</File>
  </Imports>
</Objects>
```

After this there is only `ImportantStatus` and `ImportantPrototype` in the global score accessible.

Attention: No previous definition of the global scope is delivered to the inside of the include
when filters are defined.

They are several filters that can be combinated and used. Each of them takes one name value
for the name or key.

| Rule name | Description |
|---|---|
| `IncludeProp` | Include some Prototypes |
| `IncludeObj` | Include some global Objects |
| `IncludeStat` | Include some Status keys |
| `ExcludeProp` | Includes everything except given Prototype |
| `ExcludeObj` | Includes everything except given global Object |
| `ExcludeStat` | Include everything except given Status |

In this filter you can define non existing names, but if you define a whitelist then the
correspondent blacklist is ignored.

## import path names

Each file is searched relative to the working directory of the executable (most the save directory).
The file extension `.xml` can be omit.

Additionaly some other paths are searched for the file:
- if file name is absolut so the absolute path would be checked
- `./` - current working directory
- `./lib/`
- `./ui/`
- `./visuals/`
- `./ui-temp/`

If the same file is localed under multiple directorys, so the first one in the list would used.
If the file is not found in all of these paths, then the loading of this single file aborts
(if require mode on an exception occour).

