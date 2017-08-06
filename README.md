# multiIDE 1.0.0.0 pre-alpha by Evgeniy Chaev

Emulates several virtual machines and provides sharing access to virtual IO devices.

Test 1.
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

Test 2.
	in addition to that running machines (or in new Workplace)
	create a new IDE without Console
	then click Machine > I/O Ports > #0:... > In: (empty input port) > New Console Device
	and Machine > I/O Ports > #0:... > Out: (empty output port) > [Connect To:] Console id* - ...
	and open BFS program file "\multiIDE\BFS code examples\cycle division.bfs"
	then Start running and input in Console a dividend and a divisor (with space or enter between)
	and then click Enter to get a quotient, then it goes around in cycle

Test 3. 
	close, connect and disconnect consoles and other IO devices in any order trying to crash the app

