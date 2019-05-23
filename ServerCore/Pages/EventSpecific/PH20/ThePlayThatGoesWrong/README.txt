This directory contains an example use of the sync controller:  the puzzle
"The Play That Goes Wrong" from Puzzlehunt 20.

To create and configure the puzzle, use the following steps:

1. Create a puzzle (perhaps using the title "The Play That Goes Wrong"), and
   note what its ID is for later.

2. On the "Pieces" page for that puzzle:
   a) Click the "Bulk Create" link.
   b) Copy the contents of the file pieces.txt (from this directory) into the
      text box.
   c) Click the "Bulk Create" button.

3. In the file client0001.html in this directory, replace "/3/940" with
   "/XXX/YYY" where XXX is your event's ID and YYY is the ID for the puzzle.

4. [Optional] If you want the various links to answer-submission pages in the
   puzzle to point to actual answer-submission pages, edit the client0001.html
   further, as follows.  For the first parameter in the call to
   initialize_meta(), change 940 to the ID for the puzzle.  For the third
   parameter in each call to initialize_puzzle(), use whatever puzzle ID you
   want the corresponding link to point to.  Finally, in the template string
   '/3/play/Submissions/%s', change the 3 to your event ID.

5. On the "Files" page for that puzzle, upload the file client0001.html.

6. On the "Edit" page for that puzzle:
   a) Set the MaxAnnotationKey to 400.
   b) Set the CustomURL to
      https://puzzlehunt.azurewebsites.net/{eventId}/YYY/api/Sync/client/
      where YYY is the ID for the puzzle.
   c) Click "Save properties"

The puzzle should now be operational.  Clicking its link will take you to the
puzzle website.  If you want to navigate there directly, its URL is:
https://puzzlehunt.azurewebsites.net/XXX/YYY/api/Sync/client/ where XXX is
your event ID and YYY is the puzzle ID.
