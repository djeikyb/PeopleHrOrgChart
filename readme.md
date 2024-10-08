Display a PeopleHR org chart in a table.

The viewer in the web-app is pretty hard to use.
The boxes and arrows view is cool, but at the expense of searches and filters.

Run it:

```shell
dotnet publish -c release -r osx-arm64
```

To get data, you'll need to sideload the Firefox extension.
Then browse to a PeopleHR instance and nav to the org chart page.
A copy of the org chart json will be saved to a sqlite database.
Now you can run the main desktop app!

All PathIcon data is derived from Font Awesome Free 6.6.0 SVG icons.
Thanks to Fonticons Inc. for licensing them [CC BY 4.0](https://fontawesome.com/license/free)!
