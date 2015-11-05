# Owin.Scim
OWIN SCIM 2.0 implementation

RFC 7643

RFC 7644

Looking for solid contributers to expedite this effort!  
email me:  daniel.gioulakis [at] powerdms [dot] com


Roadmap
-------

1. Finish users endpoints
  1. [x] Create  
  2. [x] Retrieve  
  3. [x] Replace  
  4. [ ] Update (Patch) (IN PROGRESS)
    1. [x] Support for path filters
  5. [x] Delete  
  6. [ ] Add support for custom canonical types / validation
2. Add groups endpoints
3. Add scim server configuration endpoints
4. Add support for bulk processing
5. Add support for querying
  1. [ ] Filtering (parsing into an expression tree is already done)
  2. [ ] Sorting
  3. [ ] Ordering
  4. [ ] Pagination
  5. [ ] Projection
6. Add more extensiblity options
7. Add authN / authZ
