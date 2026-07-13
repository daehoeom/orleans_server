namespace GrainLibrary.Resource.Model;

public interface IDataSet<T>
    where T : class
{
    // 파일에서 읽은 row를 내부 저장소(Dictionary 등)에 적재. 실패 시 false(예: 키 중복).
    bool Load(T data);

    // 모든 row가 Load된 후 호출되는 검증 단계(예: 다른 테이블 참조, 값 범위 체크). 실패 시 false.
    bool PostProcess(T data);
}
