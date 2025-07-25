; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [25 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [50 x i64] [
	i64 120698629574877762, ; 0: Mono.Android => 0x1accec39cafe242 => 24
	i64 750875890346172408, ; 1: System.Threading.Thread => 0xa6ba5a4da7d1ff8 => 19
	i64 1476839205573959279, ; 2: System.Net.Primitives.dll => 0x147ec96ece9b1e6f => 12
	i64 1513467482682125403, ; 3: Mono.Android.Runtime => 0x1500eaa8245f6c5b => 23
	i64 1537168428375924959, ; 4: System.Threading.Thread.dll => 0x15551e8a954ae0df => 19
	i64 1767386781656293639, ; 5: System.Private.Uri.dll => 0x188704e9f5582107 => 15
	i64 2287834202362508563, ; 6: System.Collections.Concurrent => 0x1fc00515e8ce7513 => 3
	i64 2497223385847772520, ; 7: System.Runtime => 0x22a7eb7046413568 => 17
	i64 3311221304742556517, ; 8: System.Numerics.Vectors.dll => 0x2df3d23ba9e2b365 => 14
	i64 3551103847008531295, ; 9: System.Private.CoreLib.dll => 0x31480e226177735f => 21
	i64 3571415421602489686, ; 10: System.Runtime.dll => 0x319037675df7e556 => 17
	i64 3716579019761409177, ; 11: netstandard.dll => 0x3393f0ed5c8c5c99 => 20
	i64 3933965368022646939, ; 12: System.Net.Requests => 0x369840a8bfadc09b => 13
	i64 5570799893513421663, ; 13: System.IO.Compression.Brotli => 0x4d4f74fcdfa6c35f => 8
	i64 5573260873512690141, ; 14: System.Security.Cryptography.dll => 0x4d58333c6e4ea1dd => 18
	i64 6357457916754632952, ; 15: _Microsoft.Android.Resource.Designer => 0x583a3a4ac2a7a0f8 => 0
	i64 6894844156784520562, ; 16: System.Numerics.Vectors => 0x5faf683aead1ad72 => 14
	i64 7270811800166795866, ; 17: System.Linq => 0x64e71ccf51a90a5a => 10
	i64 7654504624184590948, ; 18: System.Net.Http => 0x6a3a4366801b8264 => 11
	i64 7714652370974252055, ; 19: System.Private.CoreLib => 0x6b0ff375198b9c17 => 21
	i64 8064050204834738623, ; 20: System.Collections.dll => 0x6fe942efa61731bf => 5
	i64 8087206902342787202, ; 21: System.Diagnostics.DiagnosticSource => 0x703b87d46f3aa082 => 7
	i64 8167236081217502503, ; 22: Java.Interop.dll => 0x7157d9f1a9b8fd27 => 22
	i64 8185542183669246576, ; 23: System.Collections => 0x7198e33f4794aa70 => 5
	i64 8368701292315763008, ; 24: System.Security.Cryptography => 0x7423997c6fd56140 => 18
	i64 8563666267364444763, ; 25: System.Private.Uri => 0x76d841191140ca5b => 15
	i64 8626175481042262068, ; 26: Java.Interop => 0x77b654e585b55834 => 22
	i64 8725526185868997716, ; 27: System.Diagnostics.DiagnosticSource.dll => 0x79174bd613173454 => 7
	i64 9702891218465930390, ; 28: System.Collections.NonGeneric.dll => 0x86a79827b2eb3c96 => 4
	i64 9808709177481450983, ; 29: Mono.Android.dll => 0x881f890734e555e7 => 24
	i64 10038780035334861115, ; 30: System.Net.Http.dll => 0x8b50e941206af13b => 11
	i64 10785150219063592792, ; 31: System.Net.Primitives => 0x95ac8cfb68830758 => 12
	i64 11334880471425336825, ; 32: RAZR_PointCRep.Android.dll => 0x9d4d95bbe6d1c5f9 => 2
	i64 11485890710487134646, ; 33: System.Runtime.InteropServices => 0x9f6614bf0f8b71b6 => 16
	i64 11597940890313164233, ; 34: netstandard => 0xa0f429ca8d1805c9 => 20
	i64 12475113361194491050, ; 35: _Microsoft.Android.Resource.Designer.dll => 0xad2081818aba1caa => 0
	i64 12550732019250633519, ; 36: System.IO.Compression => 0xae2d28465e8e1b2f => 9
	i64 13029669184510158566, ; 37: StereoKit.dll => 0xb4d2af16d394e6e6 => 1
	i64 13343850469010654401, ; 38: Mono.Android.Runtime.dll => 0xb92ee14d854f44c1 => 23
	i64 13881769479078963060, ; 39: System.Console.dll => 0xc0a5f3cade5c6774 => 6
	i64 14461014870687870182, ; 40: System.Net.Requests.dll => 0xc8afd8683afdece6 => 13
	i64 14978342719919667747, ; 41: StereoKit => 0xcfddc35858deba23 => 1
	i64 14987728460634540364, ; 42: System.IO.Compression.dll => 0xcfff1ba06622494c => 9
	i64 15100350948730631304, ; 43: RAZR_PointCRep.Android => 0xd18f392ecad27488 => 2
	i64 15115185479366240210, ; 44: System.IO.Compression.Brotli.dll => 0xd1c3ed1c1bc467d2 => 8
	i64 15133485256822086103, ; 45: System.Linq.dll => 0xd204f0a9127dd9d7 => 10
	i64 15527772828719725935, ; 46: System.Console => 0xd77dbb1e38cd3d6f => 6
	i64 17008137082415910100, ; 47: System.Collections.NonGeneric => 0xec090a90408c8cd4 => 4
	i64 17712670374920797664, ; 48: System.Runtime.InteropServices.dll => 0xf5d00bdc38bd3de0 => 16
	i64 18245806341561545090 ; 49: System.Collections.Concurrent.dll => 0xfd3620327d587182 => 3
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [50 x i32] [
	i32 24, ; 0
	i32 19, ; 1
	i32 12, ; 2
	i32 23, ; 3
	i32 19, ; 4
	i32 15, ; 5
	i32 3, ; 6
	i32 17, ; 7
	i32 14, ; 8
	i32 21, ; 9
	i32 17, ; 10
	i32 20, ; 11
	i32 13, ; 12
	i32 8, ; 13
	i32 18, ; 14
	i32 0, ; 15
	i32 14, ; 16
	i32 10, ; 17
	i32 11, ; 18
	i32 21, ; 19
	i32 5, ; 20
	i32 7, ; 21
	i32 22, ; 22
	i32 5, ; 23
	i32 18, ; 24
	i32 15, ; 25
	i32 22, ; 26
	i32 7, ; 27
	i32 4, ; 28
	i32 24, ; 29
	i32 11, ; 30
	i32 12, ; 31
	i32 2, ; 32
	i32 16, ; 33
	i32 20, ; 34
	i32 0, ; 35
	i32 9, ; 36
	i32 1, ; 37
	i32 23, ; 38
	i32 6, ; 39
	i32 13, ; 40
	i32 1, ; 41
	i32 9, ; 42
	i32 2, ; 43
	i32 8, ; 44
	i32 10, ; 45
	i32 6, ; 46
	i32 4, ; 47
	i32 16, ; 48
	i32 3 ; 49
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
