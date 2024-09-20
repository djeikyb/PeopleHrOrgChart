Display a PeopleHR org chart in a table.

The viewer in the web-app is pretty hard to use.
The boxes and arrows view is cool, but at the expense of searches and filters.

Run it:

```shell
dotnet publish -c release -r osx-arm64
```

It assumes you have a copy of the PeopleHR json at `~/peoplehr.json`.
You can get one by watching network requests while clicking around the web ui.
