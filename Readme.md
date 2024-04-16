# ExampleCollectionView

## Filter cached data
- I create fake data with 3 Group and 50 items/group
- I have SearchBar, and CollectionView Control in this ui.
- SearchBar filter data inside CollectionView
- CollectionView ==> IsGrouped = "true"
## Problem 
I got keyboard typing inside SearchBar is Lagging when filtering
## RootCause
I think problem when CollectionView re-update making SearchBar Text lagging on typing

## Environment
I'm using .net8 inside my project

Installed Workload Id      Manifest Version      Installation Source
--------------------------------------------------------------------
maui-maccatalyst           8.0.7/8.0.100         SDK 8.0.200        
maui-ios                   8.0.7/8.0.100         SDK 8.0.200        
maui-android               8.0.7/8.0.100         SDK 8.0.200        
