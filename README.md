# Tractor
Tractor is a small program to allow custom test profiles to be rapidly created. Tractor then "drives" these profiles and 
controls the QA401 application to make the measurments. In the current check-in, Tractor can make gain, THD, noisefloor 
and IMD (ITU) measurements. 

The product is currently in alpha release. A video of an early version of the product being used is [here](https://www.youtube.com/watch?v=Jem6Rxyk630). 

Release 0.7 of the product re-factors a lot of the test classes, and provides two new tests. The first test (Prompt01) permits a picture to be shown to the operator so that the operator can make an adjustment:

![](https://user-images.githubusercontent.com/27789827/49888653-61d50c80-fdf4-11e8-9913-3c4ea62f3b5e.PNG)

The second test (Audition01) allows you have the operator loop a wav file over and over so that the operator can listen for scratchy pots, whether or not certain algorithms (such as delays with extreme feedback) might be operating correctly.

![](https://user-images.githubusercontent.com/27789827/49888652-61d50c80-fdf4-11e8-9e67-067966d372f7.png)

The database code has been implemented, and if you have a free version of SQL running, you should be able to log tests directly to the database. In the settings dlg below, you can see the buttons to help you get a database set up. If you aren't familiar with SQL databases, the key is to install SQL Management Studio. This gives a GUI for getting things set up and figuring out your connect string. With Tractor and the Management Studio running alongside each other, you should be able to create/delete and connect to databases just by providing the correct connection string. All tests, including screen grabs of results, will then be logged directly into the database.

![](https://user-images.githubusercontent.com/27789827/49889211-c9d82280-fdf5-11e8-8c1c-e431b5a7d880.png)
