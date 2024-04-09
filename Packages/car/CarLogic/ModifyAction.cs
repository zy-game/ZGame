namespace CarLogic
{
    public delegate void ModifyAction<R>(ref R r);
    public delegate void ModifyAction<T, R>(T t, ref R r);
    public delegate void ModifyAction<T1, T2, R>(T1 t1, T2 t2, ref R r);
}
