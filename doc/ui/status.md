# UI Language - Status

All possible Status combinations needs to defined anywhere root `<Objects>` (better is in 
the beginning).

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Status>
    <Value>Main</Value>
  </Status>
</Objects>
```

This is an example of only one Status. For more complex application its better to have multiple 
status definition so the whole ui and parts of the backend can work with this. Most actions 
in the ui happen when the status was changed.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Status>
    <Value>Status1</Value>
    <Value>Status2</Value>
    <Value>Status3</Value>
    <Value>Status4</Value>
  </Status>
</Objects>
```

This is an example of four status indicators. Most complex applications whould have much more 
status indicators. Sometimes its better to group some of them.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Status>
    <Value>Status1</Value>
    <Value>Status2</Value>
    <Value>StatusGroup1/Value>
    <Value>Status4</Value>
  </Status>
  <Status extends="StatusGroup1">
    <Value>SubStatus1</Value>
    <Value>SubStatus2</Value>
  </Status>
</Objects>
```

Here is a group of Status defined (named `StatusGroup1`). This Group contains 2 Status two.
Its also possible to make a sub group of a sub group:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Objects>
  <Status>
    <Value>Status1</Value>
    <Value>Status2</Value>
    <Value>StatusGroup1/Value>
    <Value>Status4</Value>
  </Status>
  <Status extends="StatusGroup1">
    <Value>StatusGroup2</Value>
    <Value>SubStatus2</Value>
  </Status>
  <Status extends="StatusGroup1|StatusGroup2">
    <Value>SubSubStatus1</Value>
    <Value>SubSubStatus2</Value>
  </Status>
</Objects>
```

In the end the following status are defined and usable:

- `Status1`
- `Status2`
- `StatusGroup1`
	- `StatusGroup1|StatusGroup2`
		- `StatusGroup1|StatusGroup2|SubSubStatus1`
		- `StatusGroup1|StatusGroup2|SubSubStatus2`
	- `StatusGroup1|SubStatus2`
- `Status4`

The `|` is delimiter between the status keys in the path. The whole key can be used anywhere
where you need to insert a status key in ui definition.

Its always recommended to define the complete status name in the status changer and for 
activators only the needed part. 

Lets say the current status is `StatusGroup1|StatusGroup2` and the former was `Status2`.
Then following would happen to the animations:

| New Animation status | Executed | Reason |
|---|---|---|
| `StatusGroup1` | true | NAS is part of current status |
| `StatusGroup1|StatusGroup2` | true | NAS is part of current status |
| `StatusGroup1|StatusGroup2|SubSubStatus1` | false | NAS is not part of current status |
