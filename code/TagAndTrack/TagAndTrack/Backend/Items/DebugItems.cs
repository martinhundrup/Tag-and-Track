using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Backend.Items
{
    internal static class DebugItems
    {
        public static string specimensCSV = @"0,ARC-000000,Salmon Specimen,Cleaned bone specimen,true
1,ARC-000001,Lion Claw,Preserved specimen stored in fluid jar,true
2,ARC-000002,Bear Femur,Egg sample in container,false
3,ARC-000003,Heron Egg,Fluid jar sample with tag,true
4,ARC-000004,Toad Sample,Full skeleton display,false
5,ARC-000005,Owl Wing,Tissue sample for genetics,true
6,ARC-000006,Beaver Skull,Cleaned bone specimen,true
7,ARC-000007,Lion Claw,Cleaned bone specimen,true
8,ARC-000008,Toad Sample,Loose bone fragment,false
9,ARC-000009,Fox Pelt,Cleaned bone specimen,false
10,ARC-000010,Eagle Feather,Full skeleton display,true
11,ARC-000011,Fox Pelt,Loose bone fragment,true
12,ARC-000012,Heron Egg,Taxidermy display specimen,false
13,ARC-000013,Salmon Specimen,Cleaned bone specimen,false
14,ARC-000014,Lion Claw,Taxidermy display specimen,true
15,ARC-000015,Eagle Feather,Loose bone fragment,true
16,ARC-000016,Fox Pelt,Egg sample in container,true
17,ARC-000017,Beaver Skull,Mounted pelt used for teaching,true
18,ARC-000018,Bear Femur,Fluid jar sample with tag,true
19,ARC-000019,Eagle Feather,Egg sample in container,false
20,ARC-000020,Eagle Feather,Partial remains specimen,false
21,ARC-000021,Heron Egg,Loose bone fragment,true
22,ARC-000022,Fox Pelt,Egg sample in container,true
23,ARC-000023,Salmon Specimen,Partial remains specimen,true
24,ARC-000024,Owl Wing,Mounted pelt used for teaching,true
25,ARC-000025,Bear Femur,Egg sample in container,true
26,ARC-000026,Wolf Skull,Cleaned bone specimen,true
27,ARC-000027,Salmon Specimen,Tissue sample for genetics,true
28,ARC-000028,Wolf Skull,Loose bone fragment,false
29,ARC-000029,Toad Sample,Taxidermy display specimen,true
30,ARC-000030,Salmon Specimen,Tissue sample for genetics,true
31,ARC-000031,Fox Pelt,Mounted pelt used for teaching,true
32,ARC-000032,Lion Claw,Egg sample in container,true
33,ARC-000033,Bear Femur,Egg sample in container,true
34,ARC-000034,Eagle Feather,Egg sample in container,true
35,ARC-000035,Owl Wing,Fluid jar sample with tag,true
36,ARC-000036,Toad Sample,Full skeleton display,true
37,ARC-000037,Toad Sample,Fluid jar sample with tag,false
38,ARC-000038,Beaver Skull,Tissue sample for genetics,true
39,ARC-000039,Owl Wing,Fluid jar sample with tag,true
40,ARC-000040,Beaver Skull,Full skeleton display,true
41,ARC-000041,Bear Femur,Fluid jar sample with tag,false
42,ARC-000042,Salmon Specimen,Loose bone fragment,false
43,ARC-000043,Salmon Specimen,Loose bone fragment,true
44,ARC-000044,Wolf Skull,Cleaned bone specimen,false
45,ARC-000045,Bear Femur,Full skeleton display,false
46,ARC-000046,Fox Pelt,Preserved specimen stored in fluid jar,true
47,ARC-000047,Lion Claw,Mounted pelt used for teaching,true
48,ARC-000048,Toad Sample,Full skeleton display,true
49,ARC-000049,Heron Egg,Taxidermy display specimen,true
50,ARC-000050,Beaver Skull,Full skeleton display,false
51,ARC-000051,Toad Sample,Fluid jar sample with tag,true
52,ARC-000052,Salmon Specimen,Mounted pelt used for teaching,true
53,ARC-000053,Toad Sample,Fluid jar sample with tag,false
54,ARC-000054,Lion Claw,Taxidermy display specimen,true
55,ARC-000055,Bear Femur,Partial remains specimen,false
56,ARC-000056,Heron Egg,Loose bone fragment,true
57,ARC-000057,Fox Pelt,Full skeleton display,false
58,ARC-000058,Beaver Skull,Taxidermy display specimen,true
59,ARC-000059,Toad Sample,Taxidermy display specimen,true
60,ARC-000060,Bear Femur,Mounted pelt used for teaching,true
61,ARC-000061,Fox Pelt,Tissue sample for genetics,true
62,ARC-000062,Beaver Skull,Partial remains specimen,true
63,ARC-000063,Bear Femur,Fluid jar sample with tag,false
64,ARC-000064,Fox Pelt,Partial remains specimen,true
65,ARC-000065,Toad Sample,Tissue sample for genetics,true
66,ARC-000066,Lion Claw,Egg sample in container,true
67,ARC-000067,Salmon Specimen,Cleaned bone specimen,true
68,ARC-000068,Fox Pelt,Loose bone fragment,true
69,ARC-000069,Lion Claw,Mounted pelt used for teaching,true
70,ARC-000070,Heron Egg,Preserved specimen stored in fluid jar,true
71,ARC-000071,Owl Wing,Cleaned bone specimen,true
72,ARC-000072,Lion Claw,Tissue sample for genetics,true
73,ARC-000073,Beaver Skull,Loose bone fragment,true
74,ARC-000074,Fox Pelt,Loose bone fragment,true
75,ARC-000075,Eagle Feather,Mounted pelt used for teaching,false
76,ARC-000076,Owl Wing,Egg sample in container,false
77,ARC-000077,Lion Claw,Cleaned bone specimen,true
78,ARC-000078,Wolf Skull,Cleaned bone specimen,true
79,ARC-000079,Toad Sample,Mounted pelt used for teaching,false
80,ARC-000080,Bear Femur,Preserved specimen stored in fluid jar,false
81,ARC-000081,Lion Claw,Cleaned bone specimen,false
82,ARC-000082,Toad Sample,Full skeleton display,false
83,ARC-000083,Eagle Feather,Taxidermy display specimen,true
84,ARC-000084,Salmon Specimen,Mounted pelt used for teaching,false
85,ARC-000085,Eagle Feather,Preserved specimen stored in fluid jar,false
86,ARC-000086,Lion Claw,Egg sample in container,false
87,ARC-000087,Bear Femur,Taxidermy display specimen,true
88,ARC-000088,Fox Pelt,Egg sample in container,true
89,ARC-000089,Wolf Skull,Fluid jar sample with tag,true
90,ARC-000090,Eagle Feather,Full skeleton display,true
91,ARC-000091,Lion Claw,Partial remains specimen,true
92,ARC-000092,Fox Pelt,Taxidermy display specimen,false
93,ARC-000093,Owl Wing,Preserved specimen stored in fluid jar,false
94,ARC-000094,Wolf Skull,Taxidermy display specimen,true
95,ARC-000095,Toad Sample,Full skeleton display,false
96,ARC-000096,Beaver Skull,Loose bone fragment,false
97,ARC-000097,Lion Claw,Mounted pelt used for teaching,true
98,ARC-000098,Lion Claw,Mounted pelt used for teaching,false
99,ARC-000099,Wolf Skull,Tissue sample for genetics,true
100,ARC-000100,Beaver Skull,Mounted pelt used for teaching,true";

        public static string loanCSV = @"0,NULL,Loan #0,Bird wing analysis loan,Dr. Lydia Finch,lydia.finch@wsu.edu,2025-11-03,2025-12-03,{81,45,73,1,59,64,83,89,70,7,27,46,98,87,28,26,14,6,5,48,67,54,11,57,63,31,60,24,78,10}
1,NULL,Loan #1,Fish skeleton research loan,Marcus Lee,marcus.lee@wsu.edu,2025-11-02,2025-12-02,{90,50,68,23,91,4,70,13,9,42,74,35,86,55,64,85,72,1,48,31,76,93,36,2,26,32}
2,NULL,Loan #2,Mammal bone comparison,Claire O'Hara,claire.ohara@wsu.edu,2025-11-01,2025-11-30,{22,51,79,1,17,68,37,98,80,95,96,11,83,21,30,47,41,71,48,43,58}
3,NULL,Loan #3,Avian feather pigment study,Dr. Tom Rivers,tom.rivers@wsu.edu,2025-11-03,2025-12-03,{99,1,35,22,57,91,15,45,11,34,64,76,71,92,44,70,39,29,67,59,30,20,2,46,85}
4,NULL,Loan #4,Reptile tissue sample loan,Elena Vasquez,elena.vasquez@wsu.edu,2025-11-03,2025-12-03,{78,56,97,49,86,60,2,54,25,36,6,58,9,59,46,75,19,92,33,7,65,79,27,74,52,31}
5,NULL,Loan #5,Comparative anatomy study,Dr. James Patel,james.patel@wsu.edu,2025-11-02,2025-12-02,{3,48,28,44,79,85,73,76,38,61,100,35,33,84,30,60,43,58,90,56,94,25,5,55,71,47,83}
6,NULL,Loan #6,Field biology loan set,Ana Torres,ana.torres@wsu.edu,2025-11-03,2025-12-03,{38,20,57,96,7,35,74,30,60,13,18,77,63,55,91,12,2,84,47,11,15,85,19,64,49}
7,NULL,Loan #7,Predator-prey skeletal comparison,Raj Singh,raj.singh@wsu.edu,2025-11-03,2025-12-03,{27,30,47,13,99,32,72,26,66,15,91,84,88,5,48,28,85,92,46,81,83,33,63,64,41,65,9,40,78}
8,NULL,Loan #8,Fish morphology workshop materials,Grace Liu,grace.liu@wsu.edu,2025-11-01,2025-11-30,{74,67,98,24,84,41,35,45,4,23,8,47,59,63,56,58,77,26,6,48,19,60}
9,NULL,Loan #9,Classroom teaching specimens,Dr. Nora Bell,nora.bell@wsu.edu,2025-11-02,2025-12-02,{91,70,86,83,19,73,28,15,10,57,95,6,35,77,30,40,5,24,14,54,7}
";
    }
}
