using Il2CppInterop.Runtime.Injection;

namespace WhiteLib {
    public class Initiator {
        public static void Init() {
            StructureCreator.Init();
            HeldBookCreator.Init();
        }

        public static void GameActiveInit() {
			ClassInjector.RegisterTypeInIl2Cpp<GhostFix>();
			ClassInjector.RegisterTypeInIl2Cpp<DoorController>();
			ClassInjector.RegisterTypeInIl2Cpp<Door>();
            Assets.Init();
		}
    }
}