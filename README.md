# multiIDE version 1.0.0.0 pre-alpha by Evgeniy Chaev

Emulates several virtual machines and provides sharing access to virtual IO devices.

	## The multiIDE solution consists of 7 projects (except test projects):

![architecture view](https://github.com/plankalkulist/multiIDE/blob/master/archview.png)

	### multiIDE.Core
		Contains multiIDE core components such as Workplace class, multiIDE form, IDE class and several common tools classes.
	### multiIDE.Commons
		Consists of interfaces for all components of the entire solution, also contains several common classes.
	### multiIDE.Machines
		Provides default set of virtual machines.
	### multiIDE.CodeEditors
		Provides default set of code editors.
	### multiIDE.IOports
		Provides default set of IO ports.
	### multiIDE.IOdevices
		Provides default set of IO devices.
	### multiIDE.Extras
		Provides default set of extra components both for a Workplace and for an IDE.

## Some integration testing

###  Test 1
	File > New Workplace
	click on New IDE button (with white sheet icon) or click in menu or press Ctrl+I
	check "Connect new Standard Console..." and click Create button or press Enter
	open BFS program file "\multiIDE\BFS code examples\cycle output numbers.bfs"
	click Run > Start or press F5 to start running the machine

	create New IDE without Std Console, and then connect Console id0 to new machine's output port
	by clicking Machine > I/O Ports > #0:... > Out:... > [Connect To:] Console id0 - ...
	open the same file again and Start running machine id1
	if warning about input ports/devices has shown, ignore it by clicking Yes

	now these machines are in concurrent running, and you can add more by the way

###  Test 2
	in addition to that running machines (or in new Workplace)
	create a new IDE without Console
	then click Machine > I/O Ports > #0:... > In: (empty input port) > New Console Device
	and Machine > I/O Ports > #0:... > Out: (empty output port) > [Connect To:] Console id* - ...
	and open BFS program file "\multiIDE\BFS code examples\cycle division.bfs"
	then Start running and input in Console a dividend and a divisor (with space or enter between)
	and then click Enter to get a quotient, then it goes around in cycle

###  Test 3
	close, connect and disconnect consoles and other IO devices in any order trying to crash the app

